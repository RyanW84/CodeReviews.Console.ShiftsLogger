namespace ConsoleFrontEnd.MenuSystem.Base;

/// <summary>
/// Generic interface for entity UI operations following Interface Segregation Principle
/// </summary>
public interface IEntityUi<T, TFilter>
    where T : class
    where TFilter : class
{
    // Core CRUD UI operations
    Task<T> CreateUiAsync();
    Task<T> UpdateUiAsync(T existingEntity);
    Task<TFilter> FilterUiAsync();

    // Display operations
    void DisplayEntitiesTable(IEnumerable<T> entities);

    // Selection operations
    Task<int> GetEntityByIdUiAsync();
    Task<int> SelectEntityUiAsync();
}
