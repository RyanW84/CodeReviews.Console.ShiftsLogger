using ConsoleFrontEnd.Models;
using ConsoleFrontEnd.Models.FilterOptions;

namespace ConsoleFrontEnd.MenuSystem;

public interface IWorkerUi
{
    Worker CreateWorkerUi();
    Worker UpdateWorkerUi(Worker existingWorker);
    WorkerFilterOptions FilterWorkersUi();
    void DisplayWorkersTable(IEnumerable<Worker> workers, int startingRowNumber = 1);
    Task<int> GetWorkerByIdUi();
    Task<int> SelectWorker();
    Task DisplayWorkersWithPaginationAsync(int initialPageNumber = 1, int pageSize = 10);
}
