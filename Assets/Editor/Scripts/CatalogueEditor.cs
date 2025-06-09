using Newtonsoft.Json;
using RedGame.Framework.EditorTools;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace MAG_I.ShopCatalogue.Editor
{
    public class CatalogueEditor : EditorWindow
    {
        private static readonly string _catalogueDataJsonPath = $"{Application.dataPath}/Resources/Data/CatalogueData.json";
        private SimpleEditorTableView<Product> _productsTableView;
        private SimpleEditorTableView<Bundle> _bundlesTableView;
        private StringBuilder _bundleItems;

        public static CatalogueData CatalogueData;
        public static HashSet<string> ProductIds;

        [MenuItem("MAG_I/Catalogue Editor")]
        public static void CatalogueEditorWindow()
        {
            CatalogueEditor wnd = GetWindow<CatalogueEditor>();
            if (File.Exists(_catalogueDataJsonPath))
            {
                string catalogueJsonData = File.ReadAllText(_catalogueDataJsonPath);
                if (catalogueJsonData != string.Empty)
                {
                    CatalogueData = JsonConvert.DeserializeObject<CatalogueData>(catalogueJsonData);
                    if (CatalogueData != null)
                    {
                        ProductIds = new();
                        foreach (Product p in CatalogueData.Products)
                        {
                            ProductIds.Add(p.Id);
                        }
                        int totalproducts = CatalogueData.Products.Count;
                    }
                }
                else
                {
                    CatalogueData = new()
                    {
                        Products = new(),
                        Bundles = new()
                    };
                    ProductIds = new();
                }
            }
            else
            {
                using FileStream stream = File.Create(_catalogueDataJsonPath);
            }
            wnd.titleContent = new GUIContent("Catalogue Editor");
        }

        private void BottomGUI()
        {
            if (GUILayout.Button("Save Data"))
            {
                File.WriteAllText(path: _catalogueDataJsonPath, contents: JsonConvert.SerializeObject(CatalogueData, Formatting.Indented));
                AssetDatabase.Refresh();
            }
        }

        public void OnGUI()
        {
            EditorGUILayout.BeginVertical();
            EditorGUILayout.Space(10);
            if (GUILayout.Button("Edit Products"))
            {
                ProductsEditor.ShowProductsEditor();
            }
            if (GUILayout.Button("Edit Bundles"))
            {
                BundlesEditor.ShowBundlesEditor();
            }
            EditorGUILayout.EndVertical();

            BottomGUI();
        }

    }
}