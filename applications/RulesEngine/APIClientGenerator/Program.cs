using System;
using System.IO;
using System.Threading.Tasks;
using NJsonSchema.CodeGeneration.TypeScript;
using NSwag;
using NSwag.CodeGeneration.CSharp;
using NSwag.CodeGeneration.TypeScript;

namespace APIClientGenerator
{
	/// <summary>
	/// Utility class used to generate:
	///   - typescript Axios client from RulesEngine classes.
	///   - csharp HttpClient wrapper from PublicApi openapi/swagger definition.
	/// </summary>
	/// <remarks>
	/// See .vscode/launch.json or Properties/launchSettings.json for example usage.
	/// </remarks>
	class Program
	{
		static async Task Main(string[] args)
		{
			if (args.Length == 0)
			{
				args = new string[]
				{
					"http://localhost:5050/swagger/v1/swagger.json",
                    // This path is for running from VSCode, if it doesn't work for you
                    // pass arguments that do :)
					"RulesEngine.Web/ClientApp/src/Rules.ts",
					"TypeScript"
				};
			}

			if (args.Length != 3)
				throw new ArgumentException("Expecting 3 arguments: URL, generatePath, language");

			var url = args[0];
			string cwd = Directory.GetCurrentDirectory();
			string pwd = cwd.Split("APIClientGenerator", 2)[0];
			var generatePath = Path.Combine(pwd, args[1]);
			var language = args[2];

			if (language != "TypeScript" && language != "CSharp")
				throw new ArgumentException("Invalid language parameter; valid values are TypeScript and CSharp");

			if (language == "TypeScript")
			{
				await GenerateTypeScriptClient(url, generatePath);
				var fileContent = File.ReadAllText(generatePath);
				//after the vite changes, this "moment"  import statement that nswag generates does not work anymore. fix it
				File.WriteAllText(generatePath, fileContent.Replace("import * as moment from 'moment';", "import moment from 'moment';"));
			}
			else
			{
				await GenerateCSharpClient(url, generatePath);
			}

			Console.WriteLine($"Completed generation of client {Path.GetFullPath(generatePath)}");
		}

		async static Task GenerateTypeScriptClient(string url, string generatePath) =>
			await GenerateClient(
				document: await OpenApiDocument.FromUrlAsync(url),
				generatePath: generatePath,
				generateCode: (OpenApiDocument document) =>
				{
					var settings = new TypeScriptClientGeneratorSettings();

					settings.TypeScriptGeneratorSettings.TypeStyle = TypeScriptTypeStyle.Class;
					settings.TypeScriptGeneratorSettings.TypeScriptVersion = 3.5M;
					settings.TypeScriptGeneratorSettings.DateTimeType = TypeScriptDateTimeType.OffsetMomentJS;

					settings.GenerateResponseClasses = true;
					settings.GenerateClientClasses = true;
					settings.GenerateClientInterfaces = false;
					settings.GenerateOptionalParameters = false;
					settings.GenerateDtoTypes = true;

					// ?settings.ExportTypes = true;
					settings.WrapDtoExceptions = false;
					settings.WrapResponses = false;
					settings.WrapResponseMethods = Array.Empty<string>();
					settings.ExceptionClass = "ApiException";
					settings.ClientBaseClass = null;
					settings.ResponseClass = "SwaggerResponse";
					settings.ProtectedMethods = Array.Empty<string>();

					settings.ClassName = "{controller}Client";
					// ? settings.ModuleName = "";
					// ? settings.Namespace = "";
					settings.Template = TypeScriptTemplate.Axios;
					settings.PromiseType = NSwag.CodeGeneration.TypeScript.PromiseType.Promise;
					settings.HttpClass = NSwag.CodeGeneration.TypeScript.HttpClass.HttpClient;
					settings.WithCredentials = false;
					settings.UseSingletonProvider = false;
					settings.InjectionTokenType = NSwag.CodeGeneration.TypeScript.InjectionTokenType.OpaqueToken;
					settings.RxJsVersion = 6.0M;
					// ? settings.NullValue = "Undefined";
					settings.ConfigurationClass = null;
					settings.UseTransformOptionsMethod = false;
					settings.UseTransformResultMethod = false;
					settings.ImportRequiredTypes = true;
					// ? settings.GenerateConstructorInterface = true;

					//   "operationGenerationMode": "MultipleClientsFromOperationId",
					//   "markOptionalProperties": true,
					//   "generateCloneMethod": false,
					//   "typeStyle": "Class",
					//   "enumStyle": "Enum",
					//   "useLeafType": false,
					//   "classTypes": [],
					//   "extendedClasses": [],
					//   "extensionCode": null,
					//   "generateDefaultValues": true,
					//   "excludedTypeNames": [],
					//   "excludedParameterNames": [],
					//   "handleReferences": false,
					//   "convertConstructorInterfaceData": false,
					//   "useGetBaseUrlMethod": false,
					//   "baseUrlTokenName": "API_BASE_URL",
					//   "queryNullValue": "",
					//   "useAbortSignal": false,
					//   "inlineNamedDictionaries": false,
					//   "inlineNamedAny": false,
					//   "templateDirectory": null,
					//   "typeNameGeneratorType": null,
					//   "propertyNameGeneratorType": null,
					//   "enumNameGeneratorType": null,
					//   "checksumCacheEnabled": false,
					//   "serviceHost": ".",
					//   "serviceSchemes": null,
					//   "output": "ClientApp/src/Rules.ts",
					//   "newLineBehavior": "Auto"


					// TODO: Set Axios and date time libs
					// import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse, CancelToken } from 'axios';

					var generator = new TypeScriptClientGenerator(document, settings);
					var code = generator.GenerateFile();

					return code;
				}
			);

		async static Task GenerateCSharpClient(string url, string generatePath) =>
			await GenerateClient(
				document: File.Exists(url) ? await OpenApiDocument.FromFileAsync(url) : await OpenApiDocument.FromUrlAsync(url),
				generatePath: generatePath,
				generateCode: (OpenApiDocument document) =>
				{
					var targetFile = Path.GetFileNameWithoutExtension(generatePath);

					var settings = new CSharpClientGeneratorSettings
					{
						UseBaseUrl = false,
						CSharpGeneratorSettings = {
							Namespace = $"Willow.RealEstate.{targetFile}.Generated"
						}
					};

					if (targetFile == "Command")
					{
						settings.ClassName = "CommandClient";
					}

					var generator = new CSharpClientGenerator(document, settings);
					var code = generator.GenerateFile();
					return code;
				}
			);

		private async static Task GenerateClient(OpenApiDocument document, string generatePath, Func<OpenApiDocument, string> generateCode)
		{
			Console.WriteLine($"Generating {generatePath}...");

			var code = generateCode(document);

			await System.IO.File.WriteAllTextAsync(generatePath, code);
		}
	}
}
