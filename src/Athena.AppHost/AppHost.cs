using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var postgresResource = builder
    .AddPostgres("postgres")
    .WithPgAdmin()
    .WithDataVolume();

var postgresDatabaseResource = postgresResource
    .AddDatabase("postgres-default", "postgres");

// Ollama
var ollamaResource = builder.AddContainer("ollama", "ollama/ollama:latest")
    .WithEnvironment("OLLAMA_MODELS", "/root/.ollama/models")
    .WithEnvironment("OLLAMA_NOPRUNE", "true")
    .WithBindMount("ollama", "/root/.ollama")
    .WithBindMount("./ollamaconfig", "/usr/config")
    .WithBindMount("./ollama-startup.sh", "/usr/local/bin/startup.sh")
    .WithHttpEndpoint(9999, 11434, "ollama")
    .WithEntrypoint("/bin/bash")
    .WithArgs("-c", "chmod +x /usr/local/bin/startup.sh && /usr/local/bin/startup.sh")
    .WithLifetime(ContainerLifetime.Persistent);

var ollamaEndpoint = ollamaResource.GetEndpoint("ollama");

var apiResource = builder.AddProject<Athena_Api>("api", launchProfileName: "aspire")
    .WithReference(postgresDatabaseResource, "postgres-default")
    .WithReference(ollamaEndpoint)
    .WaitFor(postgresDatabaseResource)
    .WaitFor(ollamaResource)
    .WithUrlForEndpoint("scalar", _ => new ResourceUrlAnnotation
    {
        DisplayText = "Scalar",
        Url = "/scalar"
    });

var apiEndpoint = apiResource.GetEndpoint("http");

// Open WebUI

var openWebUiResource = builder.AddContainer("openwebui", "ghcr.io/open-webui/open-webui:main")
    .WithEnvironment("OLLAMA_BASE_URL", apiEndpoint)
    .WithEnvironment("ANONYMIZED_TELEMETRY", "false")
    .WithVolume("open-webui", "/app/backend/data")
    .WithHttpEndpoint(8181, 8080)
    .WaitFor(ollamaResource)
    .WaitFor(apiResource);

await builder.Build().RunAsync();

