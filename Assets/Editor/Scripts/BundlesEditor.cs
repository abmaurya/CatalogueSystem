using RedGame.Framework.EditorTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MAG_I.ShopCatalogue.Editor
{
    public class BundlesEditor : EditorWindow
    {
        private SimpleEditorTableView<Bundle> _tableView;
        private static Queue<string> _lastBundlesId;

        public static void ShowBundlesEditor()
        {
            BundlesEditor window = GetWindow<BundlesEditor>();
            _lastBundlesId ??= new();
            window.titleContent = new GUIContent("Bundles Editor");
        }

        private void BottomGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Bundle"))
            {
                _lastBundlesId?.Enqueue($"B{CatalogueEditor.CatalogueData.Bundles.Count}");
                //Bundle bundle = new()
                //{
                //    Id = _lastBundlesId?.Dequeue(),
                //    Name = String.Empty,
                //    Price = 0,
                //    ShortDescription = "",
                //};
                //var ItemTypes = Enum.GetValues(typeof(EItemType)).Cast<EItemType>();
                //foreach (var itemType in ItemTypes)
                //{
                //    //Skipping All type -  this is only for sorting and filtering
                //    if (itemType == EItemType.All)
                //        continue;
                //    bundle.AddToBundle(new BundleItem
                //    {
                //        ItemType = itemType,
                //        Amount = 1
                //    });
                //}
                BundleEditor.ShowBundleItemEditorWindow(OnAddBundleEntryCallback, _lastBundlesId.Peek());
                //CatalogueEditor.CatalogueData.Bundles.Add(bundle);
            }
            EditorGUILayout.EndHorizontal();
        }

        void OnAddBundleEntryCallback(Bundle b)
        {
            CatalogueEditor.CatalogueData.Bundles.Add(b);
        }

        private SimpleEditorTableView<Bundle> CreateBundleTable()
        {
            SimpleEditorTableView<Bundle> tableView = new SimpleEditorTableView<Bundle>();

            tableView.AddColumn("ID", 50, (rect, item) =>
            {
                rect.xMin += 10;
                EditorGUI.LabelField(position: rect, label: item.Id);
            }).SetMaxWidth(30).SetTooltip("ID of the product");

            tableView.AddColumn("Name", 120, (rect, item) =>
            {
                EditorGUI.LabelField(position: rect, label: item.Name);
            }).SetAutoResize(true).SetTooltip("Bundle name")
                .SetSorting((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            tableView.AddColumn("Price", 50, (rect, item) =>
            {
                EditorGUI.LabelField(position: rect, label: item.Price.ToString());
            }).SetAutoResize(true).SetTooltip("Bundle price")
                .SetSorting((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            tableView.AddColumn("Description", 100, (rect, item) =>
            {
                GUIStyle textStyle = EditorStyles.label;
                textStyle.wordWrap = true;
                EditorGUI.LabelField(position: rect, label: item.ShortDescription.ToString(), textStyle);
            }).SetAutoResize(true).SetTooltip("Bundle description");

            tableView.AddColumn("Edit/View", 50, (rect, item) =>
            {
                GUIStyle buttonStyle = EditorStyles.miniButtonMid;
                buttonStyle.fontSize = 22;
                if (GUI.Button(rect, "✎", buttonStyle))
                {
                    BundleEditor.ShowBundleItemEditorWindow(ref item);
                }
            }, maxWidth: 100).SetTooltip("Click to delete this Product");

            tableView.AddColumn("Delete", 60, (rect, item) =>
            {
                GUIStyle buttonStyle = EditorStyles.miniButtonRight;
                buttonStyle.fontSize = 22;
                buttonStyle.fontStyle = FontStyle.Bold;
                if (GUI.Button(rect, "X", buttonStyle))
                {
                    CatalogueEditor.CatalogueData.Bundles.Remove(item);
                    CatalogueEditor.ProductIds.Remove(item.Id);
                    _lastBundlesId.Enqueue(item.Id);
                }
            }, maxWidth: 20).SetTooltip("Click to delete this Product");


            return tableView;
        }

        private void OnGUI()
        {
            _tableView ??= CreateBundleTable();
            _tableView.DrawTableGUI(CatalogueEditor.CatalogueData.Bundles, rowHeight:20);
            BottomGUI();
        }
    }

    public class BundleEditor : EditorWindow
    {
        private static Bundle _bundle;
        private List<BundleItem> _itemsToRemove;
        private static Action<Bundle> _callback;

        public static void ShowBundleItemEditorWindow(ref Bundle bundle)
        {
            _bundle = bundle;
            BundleEditor window = GetWindow<BundleEditor>();
            window.titleContent = new GUIContent("Bundle Editor");
        }

        public static void ShowBundleItemEditorWindow(Action<Bundle> callback, string bundleId)
        {
            _callback = callback;
            _bundle = new()
            {
                Id = bundleId,
                Name = "Nice Bundle",
                Price = 0,
                ShortDescription = "Bundle of Niceness",
            };
            BundleEditor window = GetWindow<BundleEditor>();
            window.titleContent = new GUIContent("Bundle Editor");
        }

        private void OnGUI()
        {
            EditorGUILayout.BeginVertical();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Name: ");
            _bundle.Name = EditorGUILayout.TextField(_bundle.Name);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Price: ");
            _bundle.Price = EditorGUILayout.FloatField(_bundle.Price);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Short Description: ");
            GUIStyle textAreaStyle = EditorStyles.textArea;
            textAreaStyle.stretchHeight = true;
            textAreaStyle.stretchWidth = true;
            _bundle.ShortDescription = GUILayout.TextArea(_bundle.ShortDescription, textAreaStyle);
            EditorGUILayout.EndHorizontal();

            for (int i = 0; i < _bundle.Items.Count; i++)
            {
                EditorGUILayout.BeginHorizontal();
                var currItemType = _bundle.Items[i].ItemType;
                var chosenItemType = (EItemType)EditorGUILayout.EnumPopup(_bundle.Items[i].ItemType);
                if (chosenItemType == EItemType.All)
                {
                    EditorUtility.DisplayDialog("Error", "All type can not be selected! \nChoose other options", "OK");
                }
                _bundle.Items[i].ItemType = chosenItemType == EItemType.All? currItemType: chosenItemType;
                _bundle.Items[i].Amount = (uint)EditorGUILayout.IntField((int)_bundle.Items[i].Amount);
                if (GUILayout.Button("X"))
                {
                    _itemsToRemove ??= new();
                    _itemsToRemove.Add(_bundle.Items[i]);
                }
                EditorGUILayout.EndHorizontal();
            }
            if (_itemsToRemove != null && _itemsToRemove.Count > 0)
            {
                foreach (var item in _itemsToRemove)
                {
                    _bundle.RemoveFromBundle(item);
                }
                _itemsToRemove.Clear();
            }
            if (GUILayout.Button("Add Item"))
            {
                _bundle.AddToBundle(new BundleItem
                {
                    ItemType = EItemType.Coins,
                    Amount = 1
                });
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Ok"))
            {
                _callback(_bundle);
                Close();
            }
            EditorGUILayout.EndVertical();
        }

    }
}