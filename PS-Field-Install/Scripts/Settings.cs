#define WORK
// #define HOME

namespace PS_Field_Install.Scripts {

	public static class Settings {
#if WORK
		public static string ImagesFolder_Lithonia = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\Lithonia";
		public static string ImagesFolder_PowerSentry = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\Power Sentry";

		public static string DatabaseFile = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\PowerSearch.xml";

		public static string SavedCategories = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\Categories.txt";
		public static string ResultsDisplayOrder = @"\\cdcsrvr1\Depts\PMD\COMMON\Emergency\Apps\Field Install App\Data\DisplayOrder.txt";
#elif HOME
		public static string ImagesFolder_Lithonia = @"C:\Users\sabin\Desktop\PS FIT\Data\Lithonia";
		public static string ImagesFolder_PowerSentry = @"C:\Users\sabin\Desktop\PS FIT\Data\Power Sentry";

		public static string DatabaseFile = @"C:\Users\sabin\Desktop\PS FIT\Data\PowerSearch.xml";

		public static string SavedCategories = @"C:\Users\sabin\Desktop\PS FIT\Data\Categories.txt";
		public static string ResultsDisplayOrder = @"C:\Users\sabin\Desktop\PS FIT\Data\DisplayOrder.txt";
#else

#endif

		public static string DataTableName = "Products";
		public static string CICodeTest = "CI";
		public static string DescriptionTest = "Desc";
	}
}