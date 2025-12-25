# 📘 Kubernetes Learning Guide - Complete

I've created a comprehensive, beginner-friendly Kubernetes deployment tutorial for your microservices application!

## 📁 What Was Created

All files are in the `k8s-learning/` directory:

### 📄 Step-by-Step YAML Files (12 steps)
Each file is heavily commented to explain concepts:

1. **step1-namespace.yaml** - Create isolated environment
2. **step2-configmaps.yaml** - Store Nginx configuration
3. **step3-storage.yaml** - Request persistent storage
4. **step4-mongodb.yaml** - Deploy MongoDB database
5. **step5-redis.yaml** - Deploy Redis cache
6. **step6-elasticsearch.yaml** - Deploy Elasticsearch for logging
7. **step8-products-app.yaml** - Deploy Products microservice
8. **step9-users-app.yaml** - Deploy Users microservice
9. **step10-orders-app.yaml** - Deploy Orders microservice
10. **step11-react-app.yaml** - Deploy React frontend
11. **step12-nginx-proxy.yaml** - Deploy Nginx reverse proxy
12. **step13-expose-service.yaml** - Expose app to outside world

### 📜 Scripts
- **deploy-all.ps1** - Automated deployment script
- **cleanup.ps1** - Clean up all resources

### 📚 Documentation
- **GETTING-STARTED.md** - Start here! Complete beginner's guide
- **README.md** - Overview and deployment instructions
- **COMMANDS.md** - Quick reference for kubectl commands
- **TROUBLESHOOTING.md** - Solutions to common problems

## 🚀 Quick Start

### Option 1: Deploy Everything at Once (5 minutes)
```powershell
cd k8s-learning
.\deploy-all.ps1
```

Then access your app at: **http://localhost:30080**

### Option 2: Learn Step by Step (Recommended)
```powershell
cd k8s-learning

# Read the getting started guide
notepad GETTING-STARTED.md

# Deploy each step manually
kubectl apply -f step1-namespace.yaml
kubectl apply -f step2-configmaps.yaml
# ... and so on
```

## 🎓 What You'll Learn

### Kubernetes Core Concepts
- ✅ **Namespaces** - Isolating resources
- ✅ **Pods** - Smallest deployable units
- ✅ **Deployments** - Managing application replicas
- ✅ **Services** - Stable network endpoints
- ✅ **ConfigMaps** - Configuration management
- ✅ **PersistentVolumeClaims** - Storage requests
- ✅ **Resource Limits** - CPU and memory management
- ✅ **Health Checks** - Liveness and readiness probes
- ✅ **Service Types** - ClusterIP, NodePort, LoadBalancer

### Practical Skills
- ✅ Deploy complete microservices application
- ✅ Scale services up and down
- ✅ View logs and debug issues
- ✅ Update running applications
- ✅ Configure reverse proxy
- ✅ Manage persistent data
- ✅ Expose applications externally

## 📊 Architecture Overview

```
                    ┌─────────────────┐
                    │  localhost:30080 │
                    └────────┬────────┘
                             │
                    ┌────────▼────────┐
                    │  Nginx Proxy    │ (Routes requests)
                    └────────┬────────┘
                             │
        ┌────────────────────┼────────────────────┐
        │                    │                    │
    ┌───▼───┐          ┌─────▼─────┐       ┌─────▼─────┐
    │Products│          │   Users   │       │  Orders   │
    │  API   │          │    API    │       │    API    │
    └───┬───┘          └─────┬─────┘       └─────┬─────┘
        │                    │                    │
        └────────────────────┼────────────────────┘
                             │
        ┌────────────────────┼────────────────────┬────────┐
        │                    │                    │        │
    ┌───▼───┐          ┌─────▼─────┐       ┌──────▼────┐   │
    │MongoDB│          │   Redis   │       │ RabbitMQ  │   │
    └───────┘          └───────────┘       └───────────┘   │
                                                           │
                                                   ┌───────▼─────┐
                                                   │Elasticsearch│
                                                   └─────────────┘
```

## 🔑 Key Features

### Educational Design
- **Heavily Commented**: Every line explained
- **Progressive Learning**: Concepts build on each other
- **Real-World Example**: Complete microservices setup
- **Hands-On**: Deploy and experiment immediately

### Production-Ready Concepts
- **High Availability**: Multiple replicas
- **Health Checks**: Automatic recovery
- **Resource Management**: CPU/memory limits
- **Persistent Storage**: Data survives restarts
- **Reverse Proxy**: Single entry point
- **Service Discovery**: Automatic DNS

## 📖 Documentation Structure

```
k8s-learning/
├── GETTING-STARTED.md    ← Start here!
├── README.md             ← Overview
├── COMMANDS.md           ← kubectl reference
├── TROUBLESHOOTING.md    ← Problem solving
├── deploy-all.ps1        ← Automated deployment
├── cleanup.ps1           ← Clean up
└── step*.yaml            ← 13 deployment steps
```

## 🎯 Prerequisites

1. **Docker Desktop** with Kubernetes enabled
   - Or Minikube

2. **kubectl** installed
   ```powershell
   kubectl version --client
   ```

3. **Docker images** pushed to Docker Hub ✅
   - ravishchauhan/products-app:latest
   - ravishchauhan/user-app:latest
   - ravishchauhan/orders-app:latest
   - ravishchauhan/react-app:latest

## ⚡ Quick Commands

### View Everything
```powershell
kubectl get all -n microservices-poc
```

### View Logs
```powershell
kubectl logs -f deployment/products-app -n microservices-poc
```

### Scale Service
```powershell
kubectl scale deployment products-app --replicas=3 -n microservices-poc
```

### Port Forward
```powershell
kubectl port-forward -n microservices-poc service/nginx-proxy 8080:80
```

### Delete Everything
```powershell
kubectl delete namespace microservices-poc
```

## 🐛 Troubleshooting

If something doesn't work:

1. Check pod status:
   ```powershell
   kubectl get pods -n microservices-poc
   ```

2. View pod details:
   ```powershell
   kubectl describe pod <pod-name> -n microservices-poc
   ```

3. Check logs:
   ```powershell
   kubectl logs <pod-name> -n microservices-poc
   ```

4. See [TROUBLESHOOTING.md](k8s-learning/TROUBLESHOOTING.md) for detailed solutions

## 🎉 Success Indicators

Your deployment is successful when:

✅ All pods show "Running" status
✅ http://localhost:30080 loads the React app
✅ APIs respond at /api/products, /api/users, /api/orders
✅ Swagger docs available at /products-swagger, /users-swagger, /orders-swagger

## 📚 Learning Path

1. **Week 1**: Deploy using automated script, explore the running system
2. **Week 2**: Deploy step by step, read all comments
3. **Week 3**: Experiment with scaling, updates, rollbacks
4. **Week 4**: Try breaking things and fixing them (best way to learn!)

## 🌟 What Makes This Special

- **Learning-First Design**: Every concept explained
- **Real Application**: Not a toy example
- **Complete Stack**: Databases, APIs, frontend, proxy
- **Production Patterns**: High availability, health checks
- **Troubleshooting Included**: Common issues + solutions
- **Hands-On**: Learn by doing

## 🔗 Resources

- **Getting Started**: [k8s-learning/GETTING-STARTED.md](k8s-learning/GETTING-STARTED.md)
- **All Commands**: [k8s-learning/COMMANDS.md](k8s-learning/COMMANDS.md)
- **Troubleshooting**: [k8s-learning/TROUBLESHOOTING.md](k8s-learning/TROUBLESHOOTING.md)
- **Kubernetes Docs**: https://kubernetes.io/docs/

## 💡 Next Steps

After mastering these basics:

1. Learn about **Ingress** for advanced routing
2. Add **Monitoring** with Prometheus/Grafana
3. Implement **CI/CD** pipelines
4. Explore **Helm** for package management
5. Study **Kubernetes Operators**
6. Learn about **Service Mesh** (Istio, Linkerd)

## 🎓 Congratulations!

You now have everything you need to:
- ✅ Deploy microservices to Kubernetes
- ✅ Understand core Kubernetes concepts
- ✅ Manage and troubleshoot deployments
- ✅ Scale and update applications
- ✅ Build on this foundation for advanced topics

**Start learning**: `cd k8s-learning` and read `GETTING-STARTED.md`

Happy Learning! 🚀
