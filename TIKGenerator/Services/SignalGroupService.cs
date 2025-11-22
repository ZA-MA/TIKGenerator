using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TIKGenerator.Data;
using TIKGenerator.Models;

namespace TIKGenerator.Services
{
    public interface ISignalGroupService
    {
        Task<bool> SaveGroupAsync(SignalGroup group);
        Task<bool> UpdateGroupAsync(SignalGroup group);
        Task<List<SignalGroup>> GetAllGroupsAsync();
        Task<SignalGroup?> GetGroupByIdAsync(string id);
        Task DeleteGroupAsync(string id);
    }

    public class SignalGroupService : ISignalGroupService
    {
        private readonly DbContextOptions<AppDbContext> _options;

        public SignalGroupService(DbContextOptions<AppDbContext> options)
        {
            _options = options;
        }

        public async Task<bool> SaveGroupAsync(SignalGroup group)
        {
            try
            {
                await using var db = new AppDbContext(_options);
                await db.SignalGroups.AddAsync(group);
                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;

                return false;
            }
        }

        public async Task<bool> UpdateGroupAsync(SignalGroup group)
        {
            try
            {
                await using var db = new AppDbContext(_options);

                var existingGroup = await db.SignalGroups
                    .Include(g => g.Signals)
                    .FirstOrDefaultAsync(g => g.Id == group.Id);

                if (existingGroup == null) return false;

                existingGroup.Name = group.Name;
                existingGroup.CreatedAt = group.CreatedAt;

                db.Signals.RemoveRange(existingGroup.Signals);
                existingGroup.Signals = group.Signals;

                await db.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                return false;
            }
        }

        public async Task<List<SignalGroup>> GetAllGroupsAsync()
        {
            await using var db = new AppDbContext(_options);
            return await db.SignalGroups
                           .Include(g => g.Signals)
                           .ToListAsync();
        }

        public async Task<SignalGroup?> GetGroupByIdAsync(string id)
        {
            await using var db = new AppDbContext(_options);
            return await db.SignalGroups
                           .Include(g => g.Signals)
                           .FirstOrDefaultAsync(g => g.Id == id);
        }

        public async Task DeleteGroupAsync(string id)
        {
            await using var db = new AppDbContext(_options);
            var entity = await db.SignalGroups.FirstOrDefaultAsync(g => g.Id == id);
            if (entity != null)
            {
                db.SignalGroups.Remove(entity);
                await db.SaveChangesAsync();
            }
        }
    }
}
