namespace ConsoleFrontEnd.MenuSystem.Base;

/// <summary>
/// Generic interface for entity UI operations following Interface Segregation Principle
/// </summary>
public interface IEntityUi<T, TFilter>
    where T : class
    where TFilter : class
{
    // Core CRUD UI operations
    T CreateEntityUi();
    T UpdateEntityUi(T existingEntity);
    TFilter FilterEntityUi();
    
    // Display operations
    void DisplayEntitiesTable(IEnumerable<T> entities);
    
    // Selection operations
    int GetEntityByIdUi();
    int SelectEntityUi();
}
