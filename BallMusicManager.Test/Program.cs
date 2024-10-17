using BallMusicManager.Infrastructure;
using System.Globalization;

//var playlist = PlaylistBuilder.FromFile(new(@"D:\Musik\LGH Bälle\WB 23 MS\WB 23 MS.playlist"));
//var playlist = PlaylistBuilder.FromArchive(new(@"C:\Users\Barion\Downloads\test.zip")).ReduceOrThrow();

//playlist.Skip();
//playlist.Play();
//Console.ReadLine();

//PlaylistBuilder.ToArchive(new(@"C:\Users\Barion\Downloads\test.zip"), playlist.Songs);

Console.WriteLine(CultureInfo.CurrentCulture.CompareInfo.IsPrefix("Ma Chèrie", "mache", CompareOptions.IgnoreCase | CompareOptions.IgnoreNonSpace));