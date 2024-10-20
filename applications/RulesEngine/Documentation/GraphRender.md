Graph Rendering
====

This note explains roughly how the twin graph and model graph are rendered by the Rules Engine. It's aimed at someone wanting to take the same approach or to extract the code to use in another application.

Key files to look at
====

[The controller the serves up the graph data](../RulesEngine.Web/Controllers/TwinController.cs)

There's just one method of interest here and it returns a simple Dto consisting of an array of nodes and an array of edges:

````csharp
[HttpPost("TwinsGraph", Name = "TwinsGraph")]
[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TwinGraphDto))]
[ProducesResponseType(StatusCodes.Status404NotFound)]
[ProducesDefaultResponseType]
public async Task<IActionResult> GetTwinGraph([FromBody] string[] twinIds, int maxDistance = 1)
{
    logger.LogInformation($"GetTwinGraph {string.Join(", ", twinIds)}");
    var graph = await twinSystemService.GetTwinSystemGraph(willowEnvironment, twinIds);

    var result = TwinGraphDto.From(graph, twinIds);
    return Ok(result);
}
````

[The twin system service that generates the graph and the From method that modifies it slightly](../WillowRules/Services/TwinSystemService.cs)

````csharp
	/// <summary>
	/// Creates a subgraph including all of the nodes requested (cached on disk)
	/// </summary>
	public async Task<Graph<BasicDigitalTwinPoco, WillowRelation>> GetTwinSystemGraph(WillowEnvironment willowEnvironment, string[] twinIds)
````

Cache all the things
====
From here on down each intermediate result is cached to disk for a set period of time using a home-grown disk cache which could be replaced with any key value store, or maybe Redis. The disk cache incudes a fast parallel writer that uses a consistent hash to ensure no write write conflicts between threads. It serializes the data to/from BSON and if anything goes wrong it just declares the file missing so it gets recreated by the caller. The method signature is modelled on `IMemoryCache`. I used to use memory cache on top of this but the disk cache is so fast and memory is precious so I stopped doing that.

For an initial hit to a Twin that nobody has ever visited there may be several ADT requests and it can take 15-30s to render, but thereafter for anyone else hitting that twin or even for twins nearby that share some of the same relationships everything gets faster until ultimately its a few milliseconds per request.

For a deployment with more than one front-end web server the disk cache would ideally be a shared Azure Files resource. Rules Engine shares one cache between back-end and front-end so that the backend can prefill it with SELECT * from digital twins and relationships.


Graph walking algorithm
====
The algorithm that walks the graph to create the twin system graph can be found in the same file, look for `Queue<(string twinId, int distance, string following)> queue = new();` which is the start of the process of a graph-search following certain heuristics like isFedBy in both directions as far as possible, up the building hierarchy, bounce off a Zone entity as they model many:many relationships etc.

Front-end code
====
You will find the [front end React/Typescript code here](../RulesEngine.Web/ClientApp/src/components/graphs/TwinGraph.tsx). It uses ELK for the graph layout algorithms and ReactFlow for the rendering. ELK offers hundreds of ways to customize the layout, I've tried a small percentage of them:

````typescript
const done = await elk.layout(elements, {
    layoutOptions: {
    aspectRatio: '1.77',
    algorithm: 'org.eclipse.elk.layered',
    'org.eclipse.elk.force.temperature': '0.0001',
    'org.eclipse.elk.layered.priority.direction': '1',
    'elk.spacing.nodeNode': '25',
    'elk.layered.spacing.nodeNodeBetweenLayers': '35'
    },
    logging: false,
    measureExecutionTime: false
});
````

ReactFlow lets you customize the node and edges however you want with colors, fonts and boxes.

The front-end code uses react-query to wrap all api calls. This handles progress, errors, refresh and client-side caching all from the one `useQuery` call.

Grouping
====
The backend calculates two grouping keys and the front end decides how to use them to render clusters of nodes using either the primary group key, or when that's expanded the secondary group key.

The grouping code is [here](../WillowRules/Model/Twins/TwinGraphDto.cs) {yeah, hidden in a DTO, sorry, must fix that}. It's fairly 'heuristic' and may need adjusting.

Generated Typescript API Code
====
All of the code that calls back end APIs is provided by a Swagger generated api class. When a change is made on the backend you can use NSwagGen or the provided generator project to regenerate all of the DTOs and calling code in Typescript. 

Authentication / Authorization
====
This is whatever you implement on the controller method. In Rules Engine the Auth token from ADB2C is inserted into the API call automatically by an Axios interceptor.

Applicablity outside real estate
====
The graph walking code applies to any Willow Twin that uses the same relationship types and could be extended easily with any new relationship types in the future.

Code maturity
=====
Some of the graph code is definitely towards proof-of-concept quality. It's still evolving.  But it is all commented and I am not aware of any bugs currently. Pull requests gratefully received. The branch `feature/rules engine` tends to be about a week ahead of main.