#!/bin/bash
set -e

echo "=== Ollama Container Startup ==="
echo "Container started at: $(date)"
echo "Working directory: $(pwd)"
echo "User: $(whoami)"
echo "Available disk space:"
df -h

export OLLAMA_HOST=0.0.0.0:11434

# Check if ollama binary is available and working
echo "Checking ollama binary..."
which ollama || { echo "ERROR: ollama binary not found"; exit 1; }

# Ensure the data directory exists and has proper permissions
echo "Setting up ollama data directory..."
mkdir -p /root/.ollama
chmod 755 /root/.ollama

echo "Starting Ollama server..."
# Start ollama serve in the background
ollama serve &
OLLAMA_PID=$!

# Wait for ollama to be ready
echo "Waiting for Ollama server to be ready..."
sleep 10

# Check if model already exists
if ! ollama list | grep -q "qwen3:1.7b"; then
    echo "Pulling qwen3:1.7b model..."
    ollama pull qwen3:1.7b
    echo "Model pull completed successfully!"
else
    echo "Model qwen3:1.7b already exists, skipping pull"
fi

# Verify models are available
echo "Available models:"
ollama list

echo "=== Ollama setup completed successfully! ==="
echo "Container ready at: $(date)"

# Keep the container running by waiting for the ollama process
wait $OLLAMA_PID