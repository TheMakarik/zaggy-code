
# Тестирование

## Инструменты
- **xUnit** — тестовый фреймворк
- **FakeItEasy** — изоляция зависимостей
- **FluentAssertions** — утверждения
- **AutoFixture** — генерация тестовых данных

## Структура тестового класса
- Наследуй `IDisposable` при необходимости очистки
- Поля зависимостей вверху класса
- Общие фейки инициализируются в конструкторе
- Тестируемый сервис называй **`systemUnderTests`**

## Dummy vs Fake
- **`A.Dummy<T>()`** — для зависимостей, которые нужны конструктору, но не влияют на поведение
- **`A.Fake<T>()`** — для зависимостей, чьё поведение ты настраиваешь

```csharp
// Dummy — без настройки
var logger = A.Dummy<ILogger<UserStorage>>();

// Fake — с настройкой
var options = A.Fake<IOptions<StorageOptions>>();
A.CallTo(() => options.Value).Returns(new StorageOptions { ... });
```

## Генерация данных (AutoFixture)
```csharp
var fixture = new Fixture();
var expectedFontSize = fixture.Create<int>();
var expectedTheme = fixture.Create<string>();
```

## Структура теста
Разделяй на блоки: `// Arrange`, `// Act`, `// Assert`

## Именование
`Действие_Условие_ОжидаемыйРезультат`

```csharp
[Fact]
public async Task LoadAsync_WhenFileCorrupted_DeletesAndCreatesNewFile()
{
    // Arrange
    var logger = A.Dummy<ILogger<UserStorage>>();
    var options = A.Fake<IOptions<StorageOptions>>();
    A.CallTo(() => options.Value).Returns(new StorageOptions { ... });

    var systemUnderTests = new UserStorage(logger, options, _userDefaultMock, _stubProvider);

    // Act
    await systemUnderTests.LoadAsync();

    // Assert
    actualContent.Should().Contain(expectedUser.CodeFontSize.ToString());
}
```

## Утверждения
- Используй FluentAssertions: `.Should().Be(...)`, `.Should().Contain(...)`, `.Should().NotBeNull()`
- Избегай нескольких ассертов без веской причины

## Работа с файловой системой
Используй `TestFileSystem` из `ZaggyCode.Tests.Infrastructure`
Формируй пути через `Path.Join`, а не `Path.Combine`
