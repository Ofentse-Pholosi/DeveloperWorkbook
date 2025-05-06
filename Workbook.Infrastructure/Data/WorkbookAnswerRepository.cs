using MongoDB.Driver;
using Workbook.Core.Entities;

namespace Workbook.Infrastructure.Data
{
    public class WorkbookAnswerRepository
    {
        private readonly IMongoCollection<WorkbookAnswer> _workbookAnswers;

        public WorkbookAnswerRepository(MongoDbContext dbContext)
        {
            _workbookAnswers = dbContext.WorkbookAnswers.Database.GetCollection<WorkbookAnswer>("WorkbookAnswers");
        }

        public async Task SaveWorkbookAnswerAsync(WorkbookAnswer workbookAnswer)
        {
            await _workbookAnswers.InsertOneAsync(workbookAnswer);
        }

        public async Task<List<WorkbookAnswer>> GetWorkbookAnswersByEmailAsync(string email)
        {
            return await _workbookAnswers.Find(answer => answer.Email == email).ToListAsync();
        }
    }
}
