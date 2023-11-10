using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using Npgsql;
using System.Text.Json;
using Types;


namespace Supplier.Services
{
    public class Repository
    {
        private readonly CodeObjectContext _context;

        public Repository(CodeObjectContext context)
        {
            _context = context;
        }

        public async Task<List<CodeObject>> GetAsync()
        {
            List<CodeObject> codeObjects = await _context.METODO.Where(obj => !obj.ALREADY_EXPORTED).ToListAsync();
            return codeObjects;
        }

        public async Task<bool> UpdateAsync(CodeObject entityToUpdate)
        {
            if (entityToUpdate != null)
            {
                entityToUpdate.ALREADY_EXPORTED = true;
                _context.METODO.Update(entityToUpdate);
            }

            return true;
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }
    }
}
