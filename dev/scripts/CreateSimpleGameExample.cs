#:project ../../src/ZaggyCode.Core

#pragma warning  disable IL2026
#pragma warning  disable IL3050

using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Xml.Serialization;
using ZaggyCode.Core.Game.Enums;
using ZaggyCode.Core.Game.Models;
using Point = System.Drawing.Point;

var map1 = CreateMap(verticalLength: 3, horizontalLength: 2, startX: 9, startY: 9);
var map2 = CreateMap(verticalLength: 5, horizontalLength: 3, startX: 9, startY: 9);
var map3 = CreateMap(verticalLength: 10, horizontalLength: 6, startX: 9, startY: 9);

var game = new Game
{
    Author = "TheMakarik",
    Description = "Silly test with G-shaped maps",
    Difficulty = Difficulty.Easy,
    Name = "Test",
    Maps = [map1, map2, map3]
};

await using var stream = new MemoryStream();
var xml = new XmlSerializer(typeof(Game));
xml.Serialize(stream, game);
stream.Position = 0;

using var streamReader = new StreamReader(stream);
Console.WriteLine(await streamReader.ReadToEndAsync());

return;

Map CreateMap(int verticalLength, int horizontalLength, int startX, int startY)
{
    var map = new Map
    {
        Width = 20,
        Height = 20,
        Points = []
    };

    for (var y = 0; y < 20; y++)
    for (var x = 0; x < 20; x++)
        map.Points.Add(new Point
        {
            X = x,
            Y = y,
            WallType = WallType.Full,
            RequireDraw = false,
            IsSpawn = false,
            HasCoin = false
        });

    for (var i = 0; i < verticalLength; i++)
    {
        var currentY = startY - i;
        var index = currentY * 20 + startX;
        map.Points[index].IsSpawn = i == verticalLength - 1;
        map.Points[index].WallType = WallType.None;
        map.Points[index].RequireDraw = true;
    }

    var topY = startY - verticalLength + 1;
    for (var i = 0; i < horizontalLength; i++)
    {
        var index = topY * 20 + (startX + 1 + i);
        map.Points[index].WallType = WallType.None;
        map.Points[index].RequireDraw = true;
    }

    return map;
}