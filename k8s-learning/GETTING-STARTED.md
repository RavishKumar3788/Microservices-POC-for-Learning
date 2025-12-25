# 🚀 Getting Started with Kubernetes

This is a beginner-friendly, step-by-step guide to deploy your microservices application to Kubernetes.

## 📋 What You'll Learn

By following this tutorial, you'll understand:
- ✅ What Kubernetes is and why it's useful
- ✅ How to deploy applications to Kubernetes
- ✅ Core Kubernetes concepts (Pods, Services, Deployments)
- ✅ How to manage and troubleshoot your deployments
- ✅ How microservices communicate in Kubernetes

## 🎯 Prerequisites

### 1. Kubernetes Cluster

Choose ONE of these options:

**Option A: Docker Desktop (Easiest for Windows/Mac)**
1. Install [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Open Docker Desktop
3. Go to Settings → Kubernetes
4. Check "Enable Kubernetes"
5. Click "Apply & Restart"
6. Wait for Kubernetes to start (green icon in bottom-left)

**Option B: Minikube (Cross-platform)**
1. Install [Minikube](https://minikube.sigs.k8s.io/docs/start/)
2. Start Minikube:
   ```powershell
   minikube start
   ```

### 2. kubectl (Kubernetes CLI)

**Windows (PowerShell as Administrator)**:
```powershell
choco install kubernetes-cli
# Or download from: https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/
```

**Verify Installation**:
```powershell
kubectl version --client
```

### 3. Docker Images

Your Docker images are already pushed to Docker Hub:
- ✅ ravishchauhan/products-app:latest
- ✅ ravishchauhan/user-app:latest
- ✅ ravishchauhan/orders-app:latest
- ✅ ravishchauhan/react-app:latest

## 🏃 Quick Start (5 minutes)

### Deploy Everything Automatically

```powershell
cd k8s-learning
.\deploy-all.ps1
```

This script will:
1. Create namespace
2. Set up configurations
3. Deploy databases (MongoDB, Redis, RabbitMQ, Elasticsearch)
4. Deploy your microservices
5. Deploy frontend
6. Set up reverse proxy
7. Expose the application

Wait 3-5 minutes for everything to start, then access:
- **Application**: http://localhost:30080
- **Products API**: http://localhost:30080/api/products
- **Users API**: http://localhost:30080/api/users
- **Orders API**: http://localhost:30080/api/orders

## 📚 Learn Step by Step (Recommended)

If you want to understand each step:

### Step 1: Create Namespace
```powershell
kubectl apply -f step1-namespace.yaml
```

**What you learned**: Namespaces isolate resources

**Verify**:
```powershell
kubectl get namespaces
```

---

### Step 2: Create Configuration
```powershell
kubectl apply -f step2-configmaps.yaml
```

**What you learned**: ConfigMaps store configuration data

**Verify**:
```powershell
kubectl get configmaps -n microservices-poc
```

---

### Step 3: Create Storage
```powershell
kubectl apply -f step3-storage.yaml
```

**What you learned**: PersistentVolumeClaims request storage

**Verify**:
```powershell
kubectl get pvc -n microservices-poc
```

---

### Step 4-6: Deploy Databases
```powershell
kubectl apply -f step4-mongodb.yaml
kubectl apply -f step5-redis.yaml
kubectl apply -f step6-elasticsearch.yaml
```

**What you learned**: 
- Deployments manage pods
- Services provide stable network endpoints
- Resource limits prevent resource exhaustion

**Wait for databases (important!)**:
```powershell
kubectl get pods -n microservices-poc -w
```
Press Ctrl+C when all show "Running"

**Verify**:
```powershell
kubectl get pods -n microservices-poc
kubectl get services -n microservices-poc
```

---

### Step 7-9: Deploy Microservices
```powershell
kubectl apply -f step8-products-app.yaml
kubectl apply -f step9-users-app.yaml
kubectl apply -f step10-orders-app.yaml
```

**What you learned**:
- Running multiple replicas for high availability
- Health checks (liveness and readiness probes)
- Environment variables configuration

**Verify**:
```powershell
kubectl get pods -l tier=backend -n microservices-poc
```
You should see 6 pods (2 for each service)

---

### Step 10-11: Deploy Frontend
```powershell
kubectl apply -f step11-react-app.yaml
kubectl apply -f step12-nginx-proxy.yaml
```

**What you learned**:
- Reverse proxy pattern
- Routing requests to different services
- ConfigMap as nginx configuration

**Test locally**:
```powershell
kubectl port-forward -n microservices-poc service/nginx-proxy 8080:80
```
Open: http://localhost:8080

---

### Step 12: Expose to Outside
```powershell
kubectl apply -f step13-expose-service.yaml
```

**What you learned**:
- Service types (ClusterIP, NodePort, LoadBalancer)
- Exposing applications externally

**Access your app**:
- http://localhost:30080

---

## ✅ Verification Checklist

Check if everything is running:

```powershell
# 1. All pods should be Running
kubectl get pods -n microservices-poc

# 2. All services should have endpoints
kubectl get services -n microservices-poc
kubectl get endpoints -n microservices-poc

# 3. Check logs for errors
kubectl logs -f deployment/products-app -n microservices-poc
```

## 🎮 Try These Commands

### View Everything
```powershell
kubectl get all -n microservices-poc
```

### Scale a Service
```powershell
kubectl scale deployment products-app --replicas=3 -n microservices-poc
kubectl get pods -l app=products-app -n microservices-poc
```

### View Logs
```powershell
kubectl logs -f deployment/products-app -n microservices-poc
```

### Access Services
```powershell
# RabbitMQ Management UI
kubectl port-forward -n microservices-poc service/rabbitmq 15672:15672
# Open: http://localhost:15672 (guest/guest)

# Redis Insight
kubectl port-forward -n microservices-poc service/redis 8001:8001
# Open: http://localhost:8001

# MongoDB
kubectl port-forward -n microservices-poc service/mongodb 27017:27017
# Connect with MongoDB Compass: mongodb://localhost:27017
```

### Execute Commands in Pods
```powershell
# Open shell in MongoDB pod
kubectl exec -it deployment/mongodb -n microservices-poc -- mongosh

# Test Redis
kubectl exec -it deployment/redis -n microservices-poc -- redis-cli ping
```

## 🧹 Cleanup

When you're done learning:

```powershell
.\cleanup.ps1
```

Or manually:
```powershell
kubectl delete namespace microservices-poc
```

## 📖 Additional Resources

- **[README.md](README.md)** - Complete overview and guide
- **[COMMANDS.md](COMMANDS.md)** - Quick reference for kubectl commands
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - Solutions to common problems

## 🐛 Something Not Working?

### Common Issues

**Pods stuck in Pending**:
- Wait 2-3 minutes for storage provisioning
- Check: `kubectl describe pod <pod-name> -n microservices-poc`

**ImagePullBackOff**:
- Images not on Docker Hub yet
- Make sure you ran: `docker compose push`

**Can't access localhost:30080**:
- Wait for pods to be Running
- For Minikube, use: `minikube service nginx-proxy-external -n microservices-poc`

**Databases not starting**:
- Check disk space
- Check: `kubectl logs -f deployment/mongodb -n microservices-poc`

See [TROUBLESHOOTING.md](TROUBLESHOOTING.md) for detailed solutions.

## 🎓 Next Steps

After completing this tutorial:

1. **Experiment with scaling**:
   ```powershell
   kubectl scale deployment products-app --replicas=5 -n microservices-poc
   ```

2. **Try rolling updates**:
   ```powershell
   kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc
   ```

3. **Learn about namespaces**:
   - Deploy to different environments (dev, staging, prod)

4. **Explore monitoring**:
   - Add Prometheus and Grafana

5. **Learn about Ingress**:
   - More advanced routing and SSL/TLS

## 🌟 Congratulations!

You've successfully deployed a complete microservices application to Kubernetes!

You now understand:
- ✅ Kubernetes core concepts
- ✅ How to deploy applications
- ✅ How to scale and manage services
- ✅ How to troubleshoot issues
- ✅ Container orchestration fundamentals

Keep exploring and happy learning! 🚀
