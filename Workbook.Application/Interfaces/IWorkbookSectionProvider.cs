using Workbook.Core.Entities;

namespace Workbook.Application.Interfaces;

public interface IWorkbookSectionProvider
{
    Task<List<WorkbookSection>> GetSectionsAsync();
}
