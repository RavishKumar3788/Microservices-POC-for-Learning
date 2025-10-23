# Microservices POC for Learning

A small, opinionated proof-of-concept repository for learning the building blocks of a microservices architecture. The project collects examples, configuration and a sample frontend application to explore patterns such as messaging, centralized logging, containerization and orchestration.

Key technologies and topics covered (conceptually and in supporting artifacts):

- RabbitMQ — message brokering and asynchronous communication between services
- ELK (Elasticsearch, Logstash, Kibana) — centralized logging, search and dashboards
- Docker — container images and lightweight runtime packaging
- Kubernetes (K8s) — orchestration patterns for scaling and resilience

This repository is intended as an educational starting point. Not all services are fully production-ready; instead you'll find examples, notes and a sample React frontend (`my-app`) to experiment with.

## Repository layout

Top-level files and folders you'll find in this repo:

- `my-app/` — sample React app (create-react-app / TypeScript) with its own `Dockerfile`, `INSTRUCTIONS.md` and helpful docs
  - `docker-commands.md` — example Docker commands for the app
  - `INSTRUCTIONS.md` — app-specific notes and environment details
- `README.md` — this file

Explore `my-app` first if you want to run the demo frontend locally.

## Quick start (run the frontend locally)

Prerequisites:

- Node.js (18+ recommended)
- npm or yarn
- Optional: Docker if you want to run the containerized image

From PowerShell (Windows):

```powershell
cd my-app
npm install
npm start
```

This will start the React development server (usually on http://localhost:3000). See `my-app/INSTRUCTIONS.md` for app-specific environment variables and notes.

## Build and run with Docker

The `my-app` folder includes a `Dockerfile`. Example PowerShell commands to build and run the image locally:

```powershell
# build the image
cd my-app
docker build -t microservices-poc-my-app .

# run the container, mapping port 3000
docker run --rm -p 3000:3000 microservices-poc-my-app
```

If you'd rather see step-by-step Docker commands, open `my-app/docker-commands.md`.

## Notes on messaging, logging and orchestration

- RabbitMQ: the repo contains examples and notes to experiment with message consumers/producers. You'll need a running RabbitMQ instance (Docker image or a managed service) to test end-to-end flows.
- ELK: this POC references the ELK stack as an observability solution. You can run Elasticsearch and Kibana in Docker for experimenting with logs and visualizations.
- Kubernetes: this repo is meant for learning patterns; if you want to deploy to K8s, create manifests or a Helm chart and use `kubectl apply` against a local cluster (kind, minikube) or cloud provider.

There are no opinionated production configurations in this repo. Treat the artifacts as examples and learning material.

## Troubleshooting

- If the React dev server doesn't start, ensure Node and npm versions match the ones in `my-app/package.json` and delete `node_modules` + reinstall.
- If Docker fails to build, check the `Dockerfile` in `my-app` and confirm your Docker daemon is running.

## Contributing

Contributions, improvements and questions are welcome. For small changes, open a branch and submit a PR with a clear description of the change and why it helps the learning experience.

## Where to look next

- `my-app/INSTRUCTIONS.md` — app-specific run and env instructions
- `my-app/Dockerfile` and `my-app/docker-commands.md` — Docker examples

---

If you want, I can also:

- Add a short script or npm task to build and run the Docker image from the repository root
- Create a `docker-compose.yml` to spin up the frontend plus a RabbitMQ instance for local testing
- Draft Kubernetes manifests (Deployment, Service) for the frontend

Tell me which of the above you'd like and I will create it.
