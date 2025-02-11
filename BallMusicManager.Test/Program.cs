using Ametrin.Serialization;
using BallMusicManager.Domain;
using BallMusicManager.Infrastructure;
using System.Globalization;

SongLocation location = new UndefinedLocation();
JsonExtensions.DefaultOptions.Converters.Add(new SongLocationJsonConverter());

Console.WriteLine(location.ConvertToJson());