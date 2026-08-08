using Robot;
using System;

//Начало программы
Console.WriteLine("Hello, World!");
while(Robot.CanMoveUp()){
   Robot.MoveUp();
}
Console.WriteLine("Конец!");
