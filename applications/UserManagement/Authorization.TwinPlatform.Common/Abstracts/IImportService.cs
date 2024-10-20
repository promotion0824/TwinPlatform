using Authorization.TwinPlatform.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Authorization.TwinPlatform.Common.Abstracts;
public interface IImportService
{
	/// <summary>
	/// Method to register Permission and Roles in to Authorization Database
	/// </summary>
	/// <param name="importModel">Instance of Import Model</param>
	/// <returns>Completed Task</returns>
	public Task ImportDataFromConfiguration(ImportModel? importModel = null);

	/// <summary>
	/// Method to register configued Roles and Permission for the extension
	/// </summary>
	public Task ImportDataFromConfigLazy();

}
