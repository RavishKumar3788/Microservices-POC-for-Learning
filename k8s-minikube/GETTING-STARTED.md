# 🚀 Getting Started with Kubernetes

Hey! So you want to learn Kubernetes? Great choice. This guide will walk you through deploying a full microservices app to Kubernetes, and honestly, it's way easier than you might think.

## What You'll Actually Learn

By the end of this, you'll get:
- What Kubernetes is and why people won't shut up about it
- How to deploy real applications (not just "hello world")
- The important stuff: Pods, Services, and Deployments
- How to debug when things inevitably go wrong
- How your microservices talk to each other

No fluff, just practical stuff you'll actually use.

## 🎯 What You Need First

### 1. A Kubernetes Cluster

You need somewhere to run this. Pick whichever is easier for you:

**Option A: Docker Desktop** (I'd go with this if you're on Windows/Mac)
1. Grab [Docker Desktop](https://www.docker.com/products/docker-desktop)
2. Install it and open it up
3. Click Settings → Kubernetes
4. Check "Enable Kubernetes"
5. Hit "Apply & Restart"
6. Wait a minute... when you see the green icon at the bottom-left, you're good!

**Option B: Minikube** (works on anything)
1. Install [Minikube](https://minikube.sigs.k8s.io/docs/start/)
2. Fire it up:
   ```powershell
   minikube start
   ```

### 2. kubectl

This is how you talk to Kubernetes. Think of it like SSH for clusters.

**On Windows** (open PowerShell as Admin):
```powershell
choco install kubernetes-cli
# Or just download it from: https://kubernetes.io/docs/tasks/tools/install-kubectl-windows/
```

**Check it's working**:
```powershell
kubectl version --client
```

### 3. Docker Images

Your Docker images are already pushed to Docker Hub:
- ✅ ravishchauhan/products-app:latest
- ✅ ravishchauhan/users-app:latest
- ✅ ravishchauhan/orders-app:latest
- ✅ ravishchauhan/react-app:01

## 🏃 Quick Start (if you just want it running)

Want to skip the learning and just see it work? I got you:

```powershell
cd k8s-learning
.\deploy-all.ps1
```

This script does everything for you:
1. Sets up the namespace
2. Creates all the config
3. Spins up databases (MongoDB, Redis, Elasticsearch, etc.)
4. Deploys your microservices
5. Sets up the frontend
6. Configures the reverse proxy
7. Opens it up to the outside world

Grab a coffee, come back in 5 minutes, then open:
- **Your app**: http://localhost:30080
- **Products API**: http://localhost:30080/api/products
- **Users API**: http://localhost:30080/api/users
- **Orders API**: http://localhost:30080/api/orders

## 📚 Learn by Doing (recommended if you want to understand this stuff)

Want to actually understand what's happening? Let's go through it step by step:

### Step 1: Create Your Namespace
```powershell
kubectl apply -f step1-namespace.yaml
```

**What just happened?** Namespaces are like folders for your Kubernetes stuff. Keeps everything organized.

**See it for yourself**:
```powershell
kubectl get namespaces
```

---

### Step 2: Set Up Configuration
```powershell
kubectl apply -f step2-configmaps.yaml
```

**What just happened?** ConfigMaps hold your config data (database URLs, API keys, etc.). It's like environment variables, but fancier.

**See it**:
```powershell
kubectl get configmaps -n microservices-poc
```

---

### Step 3: Request Storage
```powershell
kubectl apply -f step3-storage.yaml
```

**What just happened?** Your databases need somewhere to store data. PersistentVolumeClaims are basically saying "hey, I need X GB of disk space."

**Check it out**:
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

## 🧹 Cleanup (when you're done)

Alright, had enough for today? Clean it all up:

```powershell
.\cleanup.ps1
```

Or if you want to do it manually:
```powershell
kubectl delete namespace microservices-poc
```

This nukes everything in that namespace. Fresh slate for next time!

## 📖 More Stuff to Read

- **[README.md](README.md)** - Full overview, all the details
- **[COMMANDS.md](COMMANDS.md)** - kubectl commands you'll actually use
- **[TROUBLESHOOTING.md](TROUBLESHOOTING.md)** - When stuff breaks (because it will)

## 🐛 Something Broke?

Don't panic. Here's the usual suspects:

**Pods stuck in "Pending"**:
- Just wait 2-3 minutes. Storage takes a sec to provision.
- Still stuck? Run: `kubectl describe pod <pod-name> -n microservices-poc`

**"ImagePullBackOff" error**:
- The Docker images might not be on Docker Hub yet
- Make sure you ran `docker compose push` first

**Can't access localhost:30080**:
- Wait for all pods to show "Running"
- Using Minikube? Try: `minikube service nginx-proxy-external -n microservices-poc`

**Databases won't start**:
- Check if you have enough disk space
- Look at the logs: `kubectl logs -f deployment/mongodb -n microservices-poc`

For more help, check [TROUBLESHOOTING.md](TROUBLESHOOTING.md).

## 🎓 What Now?

You made it! Here are some fun things to try now that you've got the basics:

1. **Play with scaling**:
   ```powershell
   kubectl scale deployment products-app --replicas=5 -n microservices-poc
   ```
   Watch your app handle more traffic. Pretty cool, right?

2. **Try a rolling update**:
   ```powershell
   kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc
   ```
   Kubernetes will gradually replace old pods with new ones. Zero downtime!

3. **Different environments**:
   - Try deploying to dev, staging, and prod namespaces
   - See how namespace isolation works in practice

4. **Add monitoring**:
   - Prometheus and Grafana are worth learning
   - You'll thank yourself later when debugging production issues

5. **Learn about Ingress**:
   - Better routing, SSL/TLS, all that good stuff

## 🌟 Nice Work!

You just deployed a complete microservices app to Kubernetes. That's no small feat!

You now know:
- How Kubernetes works (the important parts, anyway)
- How to deploy and manage applications
- How to scale and update services
- How to fix common issues
- The fundamentals of container orchestration

Keep experimenting, break things, fix them, and learn. That's how you get good at this stuff.

Questions? Hit me up! 🚀
