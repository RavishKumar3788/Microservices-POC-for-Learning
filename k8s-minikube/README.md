# Kubernetes Deployment Tutorial - Step by Step

This guide will help you deploy your microservices application to Kubernetes, learning each concept along the way.

## What is Kubernetes?

Kubernetes (K8s) is a container orchestration platform that automates deployment, scaling, and management of containerized applications.

## Core Concepts You'll Learn

1. **Namespace** - Isolated environment for your resources
2. **Deployment** - Manages your application pods
3. **Service** - Exposes your application to network traffic
4. **ConfigMap** - Stores configuration data
5. **PersistentVolumeClaim** - Requests storage for your data
6. **StatefulSet** - For stateful applications like databases

## Directory Structure

```
k8s-learning/
├── step1-namespace.yaml          # Create isolated environment
├── step2-configmaps.yaml         # Store configurations
├── step3-storage.yaml            # Request persistent storage
├── step4-mongodb.yaml            # Deploy MongoDB database
├── step5-redis.yaml              # Deploy Redis cache
├── step6-elasticsearch.yaml      # Deploy Elasticsearch
├── step7-products-app.yaml       # Deploy Products microservice (uses step8 file)
├── step8-users-app.yaml          # Deploy Users microservice (uses step9 file)
├── step9-orders-app.yaml         # Deploy Orders microservice (uses step10 file)
├── step10-react-app.yaml         # Deploy React frontend (uses step11 file)
├── step11-nginx-proxy.yaml       # Deploy Nginx reverse proxy (uses step12 file)
├── step12-expose-service.yaml    # Expose app to outside world (uses step13 file)
└── deploy-all.ps1                # Script to deploy everything
```

## Prerequisites

1. **Kubernetes Cluster** - Use Docker Desktop (enable Kubernetes) or Minikube
2. **kubectl** - Command-line tool for Kubernetes
3. **Docker Images** - Already pushed to ravishchauhan/ on Docker Hub ✓

## Quick Start

### Deploy Everything at Once
```powershell
cd k8s-learning
.\deploy-all.ps1
```

### Deploy Step by Step (Recommended for Learning)
```powershell
# Step 1: Create namespace
kubectl apply -f step1-namespace.yaml

# Step 2: Create configurations
kubectl apply -f step2-configmaps.yaml

# Step 3: Create storage
kubectl apply -f step3-storage.yaml

# Step 4-6: Deploy databases
kubectl apply -f step4-mongodb.yaml
kubectl apply -f step5-redis.yaml
kubectl apply -f step6-elasticsearch.yaml

# Wait for databases (give them 2-3 minutes)
kubectl get pods -n microservices-poc -w

# Step 7-9: Deploy microservices
kubectl7-9: Deploy microservices
kubectl apply -f step8-products-app.yaml
kubectl apply -f step9-users-app.yaml
kubectl apply -f step10-orders-app.yaml

# Step 10-11: Deploy frontend
kubectl apply -f step11-react-app.yaml
kubectl apply -f step12-nginx-proxy.yaml

# Step 12pply -f step13-expose-service.yaml
```

## Access Your Application

### Method 1: Port Forward (Easiest)
```powershell
kubectl port-forward -n microservices-poc service/nginx-proxy 8080:80
```
Then open: http://localhost:8080

### Method 2: NodePort
```powershell
# Get the port number
kubectl get service nginx-proxy -n microservices-poc

# Access at: http://localhost:<NODE-PORT>
```

## Verify Deployment

```powershell
# Check all resources
kubectl get all -n microservices-poc

# Check pods status
kubectl get pods -n microservices-poc

# Check services
kubectl get services -n microservices-poc

# View logs
kubectl logs -f deployment/products-app -n microservices-poc
```

## Cleanup

```powershell
# Delete everything
kubectl delete namespace microservices-poc
```

## Understanding Each File

Each YAML file is heavily commented to explain:
- What each section does
- Why it's needed
- How it works together with other components

Read through each file to understand Kubernetes concepts!

## Next Steps After Learning

1. Experiment with scaling: `kubectl scale deployment products-app --replicas=3 -n microservices-poc`
2. Update images: `kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc`
3. Check resource usage: `kubectl top pods -n microservices-poc`
4. View logs: `kubectl logs -f deployment/products-app -n microservices-poc`

## Common Issues and Solutions

### Pods Not Starting
```powershell
# Check pod details
kubectl describe pod <pod-name> -n microservices-poc

# Check logs
kubectl logs <pod-name> -n microservices-poc
```

### ImagePullBackOff Error
- Images are not available in Docker Hub
- Check image names match: ravishchauhan/products-app:latest

### Waiting for Databases
- Databases need time to start (2-3 minutes)
- Use: `kubectl get pods -n microservices-poc -w` to watch

## Learning Resources

- [Kubernetes Basics](https://kubernetes.io/docs/tutorials/kubernetes-basics/)
- [kubectl Cheat Sheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)
- [Understanding Pods](https://kubernetes.io/docs/concepts/workloads/pods/)
- [Understanding Services](https://kubernetes.io/docs/concepts/services-networking/service/)
