using HIV_System_API_BOs;
using HIV_System_API_DAOs.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HIV_System_API_DAOs.Implements
{
    public class ComponentTestResultDAO : IComponentTestResultDAO
    {
        private readonly HivSystemApiContext _context;
        private static ComponentTestResultDAO? _instance;

        public ComponentTestResultDAO()
        {
            _context = new HivSystemApiContext();
        }

        public static ComponentTestResultDAO Instance
        {
            get
            {
                if(_instance == null)
                {
                    _instance = new ComponentTestResultDAO();
                }
                return _instance;
            }
        }

        public async Task<List<ComponentTestResult>> GetAllTestComponent()
        {
            return await _context.ComponentTestResults.ToListAsync();
        }

        public async Task<ComponentTestResult> GetTestComponentById(int id)
        {
            return await _context.ComponentTestResults
                .Include(ct => ct.Stf)
                .Include(ct => ct.Trs)
                .FirstOrDefaultAsync(ct => ct.CtrId == id) 
                ?? throw new ArgumentException("Component Test Result not found", nameof(id));
        }

        public async Task<ComponentTestResult> AddTestComponent(ComponentTestResult componentTestResult)
        {
            await _context.ComponentTestResults.AddAsync(componentTestResult);
            if(componentTestResult == null)
            {
                throw new ArgumentNullException(nameof(componentTestResult), "Component Test Result cannot be null");
            }
            await _context.SaveChangesAsync();
            return componentTestResult;
        }

        public async Task<bool> UpdateTestComponent(ComponentTestResult componentTestResult)
        {
            var existingComponent = _context.ComponentTestResults.Find(componentTestResult.CtrId);
            if (existingComponent == null)
            {
                throw new ArgumentException("Component Test Result not found", nameof(componentTestResult.CtrId));
            }
            existingComponent.CtrName = componentTestResult.CtrName;
            existingComponent.CtrDescription = componentTestResult.CtrDescription;
            existingComponent.ResultValue = componentTestResult.ResultValue;
            existingComponent.Notes = componentTestResult.Notes;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTestComponent(int id)
        {
            var componentTestResult = await _context.ComponentTestResults.FindAsync(id);
            if (componentTestResult == null)
            {
                throw new ArgumentException("Component Test Result not found", nameof(id));
            }
            _context.ComponentTestResults.Remove(componentTestResult);
            _context.SaveChanges();
            return true;
        }
    }
}
