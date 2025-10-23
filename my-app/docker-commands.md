# Build the Docker image

docker build -t my-react-app .

# Run the container

docker run -d -p 3000:80 my-react-app

# View running containers

docker ps

# Stop the container

docker stop <container_id>
