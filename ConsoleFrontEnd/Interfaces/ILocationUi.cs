using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface ILocationUi
{
    Location CreateLocationUi();
    Location UpdateLocationUi(Location existingLocation);
    LocationFilterOptions FilterLocationsUi();
    void DisplayLocationsTable(IEnumerable<Location> locations, int startingRowNumber = 1);
    Task<int> GetLocationByIdUi();
    Task<int> SelectLocation();
    Task DisplayLocationsWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10);
}
