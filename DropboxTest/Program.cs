using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dropbox.Api;

namespace DropboxTest {
	class Program {

		private const string AccessToken = "Pxp-iufae2kAAAAAAAACftTrIqvxrZ2cVWRZdI3r043UmwkmGwYQPvNIdFnjIuJB";

		static void Main(string[] args) {
			var login = Task.Run((Func<Task>)Program.Run);
			login.Wait();

			Console.ReadKey();
		}

		public static async Task Run() {
			using (var dbx = new DropboxClient(AccessToken)) {
				var full = await dbx.Users.GetCurrentAccountAsync();
				Console.WriteLine("{0} - {1}", full.Name.DisplayName, full.Email);

				await ListRootFolder(dbx);

			}
		}

		public static async Task ListRootFolder(DropboxClient dbx) {
			var list = await dbx.Files.ListFolderAsync(string.Empty);

			foreach (var item in list.Entries.Where(i => i.IsFolder)) {
				Console.WriteLine("D {0}/", item.Name);
			}
		}

	}
}
