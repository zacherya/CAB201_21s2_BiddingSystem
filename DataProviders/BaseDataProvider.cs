using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EBuy.Models;
using EBuy.Helpers;

namespace EBuy.DataProviders
{
    public class BaseDataProvider<T>
    {
        /// <summary>
        /// Generic list of objects known as the Data Source
        /// </summary>
        private IList<T> DataSource { get; set; } = new List<T>();

        /// <summary>
        /// Simple boolean value to determine if there is value inside the data source
        /// </summary>
        public bool HasData { 
            get { return DataSource.Count() > 0; } 
        }

        /// <summary>
        /// Fetch the entities of the data source
        /// </summary>
        /// <returns>Entities of the datasource as an enumerable list of objects</returns>
        protected IEnumerable<T> GetEntities()
        {
            return DataSource;
        }

        /// <summary>
        /// Replace the objects in the data source with new ones. Destroy then replace.
        /// </summary>
        /// <param name="data">The enumerable list of objects to replace the current one</param>
        protected void SetEntities(IEnumerable<T> data)
        {
            try { DataSource = data.ToList(); }
            catch { throw new Exception($"Unable to overwrite the {typeof(T)} DataSource with new data"); }
            
        }

        /// <summary>
        /// Add a new entity of generic type to the datasource
        /// </summary>
        /// <param name="obj">The generic object type to add to the datasource</param>
        protected void AddEntity(T obj)
        {
            try { DataSource.Add(obj); }
            catch { throw new Exception($"Unable to add entity to {typeof(T)} DataSource"); }
        }

        /// <summary>
        /// Remove a specific object that has been used elsewhere from the datasource
        /// </summary>
        /// <param name="obj">The generic object type to remove from the datasource</param>
        protected void RemoveEntity(T obj)
        {
            try { DataSource.Remove(obj); }
            catch { throw new Exception($"Unable to remove entity from {typeof(T)} DataSource"); }
        }

        /// <summary>
        /// Ensure a type of object used outside the datasource exisits in the datasource
        /// </summary>
        /// <param name="obj">The generic object type to ensure exists in the data source</param>
        /// <returns></returns>
        protected bool EnsureEntity(T obj)
        {
            return DataSource.Contains(obj);
        }
    }
}
