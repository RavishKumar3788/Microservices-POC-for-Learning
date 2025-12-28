# Microservices POC for Learning

A comprehensive proof-of-concept repository demonstrating a complete microservices architecture with multiple deployment options. This project includes working microservices, frontend application, and full infrastructure setup for learning containerization, orchestration, messaging, and observability patterns.

## 🚀 Key Features

### **Microservices Architecture**
- **React Frontend** - Modern SPA with TypeScript and Tailwind CSS
- **Products Service** - .NET 8 API with full CRUD operations
- **Users Service** - .NET 8 API with authentication patterns
- **Orders Service** - .NET 10 API with order management. Used Server sent events to display the real time data on the dashboard.
- **Nginx Proxy** - Reverse proxy and API gateway

### **Infrastructure & Tools**
- **MongoDB** - Primary database for all services
- **Redis** - Distributed caching layer
- **RabbitMQ** - Message broker for async communication
- **ELK Stack** - Elasticsearch, Logstash, Kibana for logging
- **Docker Compose** - Local development environment
- **Kubernetes** - Production-grade orchestration

### **Multiple Deployment Options**
1. **Local Development** - Run services individually
2. **Docker Compose** - Full stack with one command
3. **Kubernetes (Local)** - Learn K8s with Minikube/Docker Desktop
4. **AWS EKS** - Production deployment on AWS cloud

## 📁 Repository Structure

```
├── my-app/                    # React frontend application
│   ├── Dockerfile
│   ├── INSTRUCTIONS.md
│   └── docker-commands.md
├── Products/                  # .NET Products microservice
│   ├── Dockerfile
│   └── Products/
├── Users/                     # .NET Users microservice
│   ├── Dockerfile
│   └── Users/
├── Orders/                    # .NET Orders microservice
│   ├── Dockerfile
│   └── Orders/
├── k8s-learning/             # Step-by-step Kubernetes tutorial
│   ├── GETTING-STARTED.md
│   ├── deploy-all.ps1
│   └── step*.yaml
├── eks-deployment/           # AWS EKS production deployment
│   ├── apps/
│   ├── infrastructure/
│   └── deploy-all.ps1
├── eks-deployment-dev/       # EKS development environment
├── docker-compose.yml        # Complete local stack
├── nginx.conf                # Nginx configuration
├── logstash.conf            # Logstash pipeline
└── KUBERNETES-LEARNING.md   # K8s learning guide
```

## 🚦 Quick Start Options

### Option 1: Docker Compose (Recommended for Quick Start)

Run the entire stack with all services in one command:

```powershell
# Start all services (React, APIs, MongoDB, Redis, ELK, RabbitMQ)
docker-compose up -d

# Access the application
# Frontend: http://localhost:8080
# Products API: http://localhost:8080/api/products
# Users API: http://localhost:8080/api/users
# Orders API: http://localhost:8080/api/orders
# Redis Insight: http://localhost:8001
# Kibana: http://localhost:5601
# RabbitMQ Management: http://localhost:15672

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

**Services included:**
- React frontend with Nginx
- Products, Users, Orders APIs (.NET 8)
- MongoDB database
- Redis cache with Redis Insight
- Elasticsearch + Logstash + Kibana
- RabbitMQ with management UI
- Nginx reverse proxy

### Option 2: Kubernetes Learning Path

Perfect for learning Kubernetes from scratch:

```powershell
cd k8s-learning

# Read the getting started guide
notepad GETTING-STARTED.md

# Deploy everything automatically
.\deploy-all.ps1

# Access at http://localhost:30080
```

📚 See [k8s-learning/GETTING-STARTED.md](k8s-learning/GETTING-STARTED.md) for step-by-step tutorial.

### Option 3: AWS EKS Production Deployment

Deploy to AWS cloud with full production setup:

```powershell
cd eks-deployment

# Follow the comprehensive guide
notepad README.md

# Prerequisites: AWS CLI, eksctl, kubectl, Helm

# Deploy to EKS
.\deploy-all.ps1
```

📚 See [eks-deployment/README.md](eks-deployment/README.md) for complete AWS deployment guide.

### Option 4: Run Individual Services Locally

For development and debugging:

**Frontend:**
```powershell
cd my-app
npm install
npm start
# Access: http://localhost:3000
```

**Products API:**
```powershell
cd Products
dotnet run --project Products
```

**Users API:**
```powershell
cd Users
dotnet run --project Users
```

**Orders API:**
```powershell
cd Orders
dotnet run --project Orders
```

## 🏗️ Architecture Overview

```
                    Internet / Load Balancer
                             ↓
                    ┌─────────────────┐
                    │  Nginx Proxy    │ (Port 8080)
                    │  (API Gateway)  │
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
    ┌───▼────┐         ┌─────▼─────┐      ┌──────▼─────┐
    │Products│         │   Users   │      │   Orders   │
    │  API   │         │    API    │      │    API     │
    │ .NET 8 │         │  .NET 8   │      │  .NET 8    │
    └───┬────┘         └─────┬─────┘      └──────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┬──────────┐
        │                    │                    │          │
    ┌───▼────┐         ┌─────▼─────┐      ┌──────▼─────┐   │
    │MongoDB │         │   Redis   │      │ RabbitMQ   │   │
    │        │         │  (Cache)  │      │ (Message)  │   │
    └────────┘         └───────────┘      └────────────┘   │
                                                            │
                                                     ┌──────▼──────┐
                                                     │ ELK Stack   │
                                                     │ (Logs)      │
                                                     └─────────────┘
```

## 🛠️ Technology Stack

### Frontend
- **React 18** with TypeScript
- **Tailwind CSS** for styling
- **Axios** for HTTP requests
- Create React App tooling

### Backend Services
- **.NET 8** Web APIs
- **MongoDB Driver** for data persistence
- **StackExchange.Redis** for caching
- **RabbitMQ Client** for messaging
- **Serilog** with Elasticsearch sink for logging

### Infrastructure
- **MongoDB** - Document database
- **Redis Stack** - Cache + Redis Insight
- **RabbitMQ** - Message broker
- **Elasticsearch 7.17** - Log storage and search
- **Logstash 7.17** - Log processing
- **Kibana 7.17** - Log visualization

### DevOps & Orchestration
- **Docker** & **Docker Compose**
- **Kubernetes** (K8s)
- **AWS EKS** - Managed Kubernetes
- **AWS ALB** - Application Load Balancer
- **EBS** - Persistent volumes
- **Nginx** - Reverse proxy

## 📚 Learning Resources

### Kubernetes Learning
- [KUBERNETES-LEARNING.md](KUBERNETES-LEARNING.md) - Overview and architecture
- [k8s-learning/GETTING-STARTED.md](k8s-learning/GETTING-STARTED.md) - Beginner's tutorial
- [k8s-learning/COMMANDS.md](k8s-learning/COMMANDS.md) - kubectl cheat sheet
- [k8s-learning/TROUBLESHOOTING.md](k8s-learning/TROUBLESHOOTING.md) - Common issues

### AWS Deployment
- [eks-deployment/README.md](eks-deployment/README.md) - Complete EKS guide
- [eks-deployment/QUICKSTART.md](eks-deployment/QUICKSTART.md) - Quick deployment

### Application Guides
- [my-app/INSTRUCTIONS.md](my-app/INSTRUCTIONS.md) - React app setup
- [my-app/docker-commands.md](my-app/docker-commands.md) - Docker commands

## 🎓 What You'll Learn

### Microservices Patterns
- ✅ Service decomposition and boundaries
- ✅ API Gateway pattern (Nginx reverse proxy)
- ✅ Database per service
- ✅ Distributed caching strategies
- ✅ Asynchronous messaging with RabbitMQ
- ✅ Centralized logging with ELK

### Containerization
- ✅ Writing Dockerfiles for different tech stacks
- ✅ Multi-stage builds for optimization
- ✅ Docker networking and volumes
- ✅ Docker Compose for multi-container apps

### Kubernetes Orchestration
- ✅ Pods, Deployments, Services
- ✅ ConfigMaps and Secrets
- ✅ Persistent storage with PVCs
- ✅ Health checks (liveness/readiness probes)
- ✅ Resource limits and requests
- ✅ Ingress and load balancing
- ✅ Scaling strategies

### Cloud Deployment
- ✅ AWS EKS cluster management
- ✅ Application Load Balancer setup
- ✅ IAM roles and security
- ✅ EBS volume provisioning
- ✅ Cost optimization strategies

## 🔧 Prerequisites

### For Docker Compose
- Docker Desktop (Windows/Mac) or Docker Engine (Linux)
- Docker Compose v2+

### For Kubernetes
- kubectl CLI
- Minikube or Docker Desktop with Kubernetes enabled
- (Optional) Helm 3

### For AWS EKS
- AWS CLI v2
- eksctl
- kubectl
- Helm 3
- AWS account with appropriate permissions

### For Development
- Node.js 18+ (for React app)
- .NET 8 SDK (for APIs)
- PowerShell 7+ (recommended)

## 🐛 Troubleshooting

### Docker Compose Issues
```powershell
# Check service logs
docker-compose logs [service-name]

# Restart a specific service
docker-compose restart [service-name]

# Rebuild images
docker-compose build --no-cache

# Remove all containers and volumes
docker-compose down -v
```

### Kubernetes Issues
See [k8s-learning/TROUBLESHOOTING.md](k8s-learning/TROUBLESHOOTING.md) for detailed solutions.

### Application Issues
- **MongoDB connection**: Ensure MongoDB is running and accessible
- **Redis connection**: Check Redis is available on port 6379
- **API not responding**: Verify service is running and firewall rules
- **Frontend can't reach API**: Check Nginx proxy configuration

## 🤝 Contributing

Contributions are welcome! This is a learning-focused repository, so improvements that enhance the educational value are especially appreciated.

### How to Contribute
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes with clear commit messages
4. Test your changes locally
5. Submit a Pull Request with a detailed description

### Areas for Contribution
- Additional microservices examples
- Enhanced documentation and tutorials
- Bug fixes and optimizations
- New deployment configurations
- Testing strategies and examples
- CI/CD pipeline templates

## 📝 License

This project is for educational purposes. Feel free to use it for learning and reference.

## 🔗 Related Resources

### Documentation
- [Docker Documentation](https://docs.docker.com/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [React Documentation](https://react.dev/)
- [MongoDB Documentation](https://docs.mongodb.com/)
- [Redis Documentation](https://redis.io/docs/)

### Tools
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Minikube](https://minikube.sigs.k8s.io/)
- [kubectl](https://kubernetes.io/docs/tasks/tools/)
- [Visual Studio Code](https://code.visualstudio.com/)

## 💡 Next Steps

After exploring this repository, you might want to:

1. **Enhance the Microservices**
   - Add authentication and authorization
   - Implement circuit breakers with Polly
   - Add distributed tracing with OpenTelemetry
   - Implement API versioning

2. **Improve Observability**
   - Set up Prometheus and Grafana for metrics
   - Configure distributed tracing
   - Create custom Kibana dashboards
   - Implement health check endpoints

3. **Add CI/CD**
   - Create GitHub Actions workflows
   - Set up automated testing
   - Implement deployment pipelines
   - Add container security scanning

4. **Scale the Architecture**
   - Implement service mesh (Istio/Linkerd)
   - Add event sourcing patterns
   - Implement CQRS
   - Add API rate limiting

## 📞 Support

If you encounter issues or have questions:
- Check the troubleshooting guides in each deployment folder
- Review the learning documentation
- Open an issue with detailed information about your problem

---

**Happy Learning! 🚀**

*This repository is designed to help you understand microservices architecture through hands-on experience. Start simple, experiment often, and gradually explore more complex patterns.*
