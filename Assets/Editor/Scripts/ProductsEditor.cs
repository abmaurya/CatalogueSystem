using RedGame.Framework.EditorTools;
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MAG_I.ShopCatalogue.Editor
{
    public class ProductsEditor : EditorWindow
    {
        private SimpleEditorTableView<Product> _tableView;
        private static Queue<string> _lastProductId;


        public static void ShowProductsEditor()
        {
            ProductsEditor window = GetWindow<ProductsEditor>();
            _lastProductId ??= new();
            window.titleContent = new GUIContent("Products Editor");
        }

        void BottomGUI()
        {
            if (GUILayout.Button("Add Product"))
            {
                _lastProductId?.Enqueue($"P{CatalogueEditor.CatalogueData.Products.Count}");
                CatalogueEditor.CatalogueData.Products.Add(
                    new Product
                    {
                        Id = _lastProductId?.Dequeue(),
                        Name = String.Empty,
                        ItemType = EItemType.Coins,
                        Price = 0,
                        ShortDescription = "",
                        Amount = 1
                    });
            }
        }

        private SimpleEditorTableView<Product> CreateProductTable()
        {
            SimpleEditorTableView<Product> tableView = new SimpleEditorTableView<Product>();

            GUIStyle labelGUIStyle = new(GUI.skin.label)
            {
                padding = new RectOffset(left: 10, right: 10, top: 2, bottom: 2)
            };

            GUIStyle disabledLabelGUIStyle = new(labelGUIStyle)
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
                var currItemType = item.ItemType;
                var chosenItemType = (EItemType)EditorGUI.EnumPopup(
                    position: labelRect,
                    item.ItemType
                );
                if(chosenItemType == EItemType.All)
                {
                    EditorUtility.DisplayDialog("Error", "All type can not be selected! \nChoose other options", "OK");
                }
                item.ItemType = chosenItemType == EItemType.All ? currItemType : chosenItemType;
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
                    CatalogueEditor.CatalogueData.Products.Remove(item);
                    CatalogueEditor.ProductIds.Remove(item.Id);
                    _lastProductId.Enqueue(item.Id);
                }
            }).SetTooltip("Click to delete this Product");

            return tableView;
        }

        private void OnGUI()
        {
            _tableView ??= CreateProductTable();
            _tableView.DrawTableGUI(CatalogueEditor.CatalogueData.Products);
            BottomGUI();
        }
    }

}