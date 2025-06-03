using RedGame.Framework.EditorTools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal.VersionControl;
using UnityEngine;
//using UnityEngine.UIElements;

namespace MAG_I.ShopCatalogue.Editor
{
    public class CatalogueEditor : EditorWindow
    {
        private static readonly string _catalogueDataJsonPath = $"{Application.dataPath}/Resources/Data/CatalogueData.json";
        private static CatalogueData _catalogueData;
        private static HashSet<string> _productIds;
        private static string _lastProductId;
        private SimpleEditorTableView<Product> _productsTableView;

        [MenuItem("MAG_I/Catalogue Editor")]
        public static void CatalogueEditorWindow()
        {
            CatalogueEditor wnd = GetWindow<CatalogueEditor>();
            if (File.Exists(_catalogueDataJsonPath))
            {
                string catalogueJsonData = File.ReadAllText(_catalogueDataJsonPath);
                if (catalogueJsonData != string.Empty)
                {
                    _catalogueData = JsonUtility.FromJson<CatalogueData>(catalogueJsonData);
                    if (_catalogueData != null)
                    {
                        _productIds = new();
                        foreach (Product p in _catalogueData.Products)
                        {
                            _productIds.Add(p.Id);
                        }
                        int totalproducts = _catalogueData.Products.Count;
                        if (totalproducts > 0)
                        {
                            _lastProductId = _catalogueData.Products[totalproducts - 1].Id;
                        }
                    }
                }
                else
                {
                    _catalogueData = new()
                    {
                        Products = new(),
                        Bundles = new()
                    };
                    _productIds = new();
                }
            }
            else
            {
                using FileStream stream = File.Create(_catalogueDataJsonPath);
            }
            wnd.titleContent = new GUIContent("Catalogue Editor");
        }

        private SimpleEditorTableView<Product> CreateProductTable()
        {
            SimpleEditorTableView<Product> tableView = new SimpleEditorTableView<Product>();

            GUIStyle labelGUIStyle = new GUIStyle(GUI.skin.label)
            {
                padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
            };

            GUIStyle disabledLabelGUIStyle = new GUIStyle(labelGUIStyle)
            {
                normal = new GUIStyleState
                {
                    textColor = Color.gray
                }
            };

            tableView.AddColumn("ID", 20, (rect, item) =>
            {
                rect.xMin += 10;
                EditorGUI.LabelField(position: rect, label: item.Id);
            }).SetMaxWidth(30).SetTooltip("ID of the product");

            tableView.AddColumn("Type", 50, (rect, item) =>
            {
                float iconSize = rect.height;
                Rect iconRect = new(rect.x, rect.y, iconSize, iconSize);
                Rect labelRect = new(iconRect.xMax, rect.y, rect.width - iconSize, rect.height);

                item.ItemType = (EItemType)EditorGUI.EnumPopup(
                    position: labelRect,
                    item.ItemType
                );
            }).SetAllowToggleVisibility(true).SetSorting((a, b) => a.ItemType - b.ItemType);

            tableView.AddColumn("Name", 50, (rect, item) =>
            {
                item.Name = EditorGUI.TextField(
                    position: rect,
                    text: item.Name
                );
            }).SetAutoResize(true).SetTooltip("Product name")
                .SetSorting((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            tableView.AddColumn("Price", 30, (rect, item) =>
            {
                item.Price = EditorGUI.FloatField(
                    position: rect,
                    value: Mathf.Abs(item.Price)
                );
            }).SetAutoResize(true).SetTooltip("Product price")
                .SetSorting((a, b) => String.Compare(a.Name, b.Name, StringComparison.Ordinal));

            tableView.AddColumn("Amount", 20, (rect, item) =>
            {
                item.Amount = (uint)EditorGUI.IntField(
                    position: rect,
                    value: (int)item.Amount
                );
            }).SetAutoResize(true).SetTooltip("Product amount");

            tableView.AddColumn("Description", 100, (rect, item) =>
            {
                item.ShortDescription = EditorGUI.TextArea(
                    position: rect,
                    text: item.ShortDescription
                );
            }).SetAutoResize(true).SetTooltip("Product description");

            tableView.AddColumn("Edit", 40, (rect, item) =>
            {
                if (GUI.Button(rect, "Delete"))
                {
                    _catalogueData.Products.Remove(item);
                    _productIds.Remove(item.Id);
                    _lastProductId = item.Id;
                }
            }).SetTooltip("Click to delete this Product");

            return tableView;
        }

        private void BottomGUI()
        {
            EditorGUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("Add Product"))
            {
                if (String.IsNullOrEmpty(_lastProductId))
                {
                    _lastProductId = $"P{_catalogueData.Products.Count}";
                }
                _catalogueData.Products.Add(
                    new Product
                    {
                        Id = _lastProductId,
                        Name = String.Empty,
                        ItemType = EItemType.Coins,
                        Price = 0,
                        ShortDescription = "",
                        Amount = 1
                    });
                _lastProductId = String.Empty;
            }
            if (GUILayout.Button("Save Data"))
            {
                File.WriteAllText(path: _catalogueDataJsonPath, contents: EditorJsonUtility.ToJson(_catalogueData, true));
            }

            EditorGUILayout.EndHorizontal();
        }

        public void OnGUI()
        {
            _productsTableView ??= CreateProductTable();

            _productsTableView.DrawTableGUI(_catalogueData.Products);

            BottomGUI();
        }
    }
}