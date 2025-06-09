# CatalogueSystem
Test project for MAG Interactive

Catalogue System:
ICatalogue:
This is an interface that can be used by different modules without knowing which class implements this. 
It contains following functions:
GetAllItems, FilterAllItems, SortItems, SortAllItemsByCustomOrder
GetAllProducts, GetAllBundles, FilterAllProducts, FilterAllBundles
FilterFromGivenItems, SortGivenItems, SortGivenItemsByCustomOrder
(Although explanation/uses of each function are in code as documentation, the function names are self-explanatory)

CatalogueManager:
This is the class that implemented ICatalogue.
In addition to that, it has a few extra functions including:
LoadCatalogueFromJson: 
A dependency injector can create a reference to CatalogueManager class and pass on to any module that needs ICatalogue.
All you need to do is pass in the the json string to CatalogueManager via LoadCatalogueManager function to initialise/populate the catalogue.

CatalogueData.cs:
This script contains:
CatalogueItem: an abstract class that is implemented by classes:
1.	Product
2.	Bundle
CatalogueItem contains data like price, name, description, and an id (future proofing!).
CatalogueData: This class contains a list of Bundle and Product.
Enum EItemType defines the type of each Product and BundleItem (a class that defines each item in a Bundle).

The menu MAG_I > Catalogue Editor gives you an option to edit the catalogue json.
