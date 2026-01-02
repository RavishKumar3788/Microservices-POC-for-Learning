# Microservices on AWS EKS

**Perfect for learning sessions**

This deployment is specifically designed for learning Kubernetes and microservices on AWS EKS. All manifest files include extensive inline documentation to help you understand what each component does.

## 📋 What's Included

### **Application Services**
- **Products API** - Product catalog management (.NET microservice)
- **Users API** - User account management (.NET microservice)
- **Orders API** - Order processing (.NET microservice)
- **React Frontend** - User interface (React + TypeScript)
- **Nginx Proxy** - API Gateway and reverse proxy

### **Infrastructure Services**
- **MongoDB** - Document database (2 GB storage)
- **Redis** - Cache layer (1 GB storage)
- ✗ **Elasticsearch** - Skipped for simplicity (use `kubectl logs` instead)

### **AWS Resources**
- Application Load Balancer (ALB) - Internet access
- EBS Volumes - Persistent storage (3 GB total)
- All running on your EKS cluster

## ✅ Prerequisites

### Required Tools
- **kubectl** (v1.28+) - [Install Guide](https://kubernetes.io/docs/tasks/tools/)
- **AWS CLI** (v2) - [Install Guide](https://aws.amazon.com/cli/)
- **eksctl** - [Install Guide](https://eksctl.io/installation/)
- **PowerShell** or **Bash** - For running scripts

### AWS Requirements
- AWS account with permissions for EKS, EC2, ELB, EBS
- AWS CLI configured (`aws configure`)
- EKS cluster running (see setup below)

### Verify Installation
```powershell
kubectl version --client
aws --version
eksctl version
aws sts get-caller-identity
```

## 🚀 Quick Start (30 minutes)

### Step 1: Create EKS Cluster

```powershell
# Create minimal cluster for learning
eksctl create cluster `
  --name microservices-learning `
  --region ap-south-1 `
  --nodegroup-name dev-nodes `
  --node-type t3.small `
  --nodes 2 `
  --nodes-min 2 `
  --nodes-max 3 `
  --managed
```

**Configuration Details:**
- **Instance Type:** t3.small (2 vCPU, 2 GB RAM) - Smallest for microservices
- **Node Count:** 2 nodes (sufficient for all services)
- **Deployment Time:** 15-20 minutes

### Step 2: Install AWS Load Balancer Controller

```powershell
# Associate OIDC provider
eksctl utils associate-iam-oidc-provider `
  --region ap-south-1 `
  --cluster microservices-learning `
  --approve

# Download and create IAM policy
Invoke-WebRequest -Uri https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/v2.6.0/docs/install/iam_policy.json -OutFile iam_policy.json

aws iam create-policy `
  --policy-name AWSLoadBalancerControllerIAMPolicy `
  --policy-document file://iam_policy.json

# Get your AWS account ID
$ACCOUNT_ID = (aws sts get-caller-identity --query Account --output text)

# Create service account
eksctl create iamserviceaccount `
  --cluster=microservices-learning `
  --namespace=kube-system `
  --name=aws-load-balancer-controller `
  --attach-policy-arn=arn:aws:iam::${ACCOUNT_ID}:policy/AWSLoadBalancerControllerIAMPolicy `
  --override-existing-serviceaccounts `
  --region ap-south-1 `
  --approve

# Install via Helm
helm repo add eks https://aws.github.io/eks-charts
helm repo update

helm install aws-load-balancer-controller eks/aws-load-balancer-controller `
  -n kube-system `
  --set clusterName=microservices-learning `
  --set serviceAccount.create=false `
  --set serviceAccount.name=aws-load-balancer-controller

# Verify
kubectl get deployment -n kube-system aws-load-balancer-controller
```

### Step 3: Install EBS CSI Driver

```powershell
# Create IAM role
eksctl create iamserviceaccount `
  --name ebs-csi-controller-sa `
  --namespace kube-system `
  --cluster microservices-learning `
  --region ap-south-1 `
  --attach-policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy `
  --approve `
  --role-only `
  --role-name AmazonEKS_EBS_CSI_DriverRole

# Install addon
aws eks create-addon `
  --cluster-name microservices-learning `
  --addon-name aws-ebs-csi-driver `
  --service-account-role-arn arn:aws:iam::${ACCOUNT_ID}:role/AmazonEKS_EBS_CSI_DriverRole `
  --region ap-south-1

# Verify
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-ebs-csi-driver
```

### Step 4: Configure kubectl

```powershell
# Update kubeconfig
aws eks update-kubeconfig --region ap-south-1 --name microservices-learning

# Verify connection
kubectl cluster-info
kubectl get nodes
```

### Step 5: Deploy Application

```powershell
# Navigate to deployment directory
cd eks-deployment-dev

# Deploy everything (3-5 minutes)
.\deploy-all.ps1 -WaitForReady
```

The script will:
1. ✓ Create namespace
2. ✓ Create ConfigMaps and storage
3. ✓ Deploy MongoDB and Redis
4. ✓ Deploy microservices
5. ✓ Deploy Nginx proxy
6. ✓ Create AWS Load Balancer
7. ✓ Display application URL

### Step 6: Access Your Application

After deployment completes, you'll see:

```
APPLICATION IS ACCESSIBLE!
Load Balancer URL:
  http://k8s-microser-xxx-123456789.ap-south-1.elb.amazonaws.com

Test Endpoints:
  Health Check:  http://[url]/nginx-health
  Frontend:      http://[url]/
  Products API:  http://[url]/api/products
  Users API:     http://[url]/api/users
  Orders API:    http://[url]/api/orders
```

## 📚 Learning Resources

### Understanding the Architecture

Every YAML file includes extensive comments explaining:
- **What each resource does**
- **Why it's configured that way**
- **How it works in Kubernetes**
- **How it integrates with other services**

Start reading from:
1. [base/namespace.yaml](base/namespace.yaml) - Understanding namespaces
2. [base/storage.yaml](base/storage.yaml) - Persistent storage concepts
3. [infrastructure/mongodb.yaml](infrastructure/mongodb.yaml) - Database deployment
4. [apps/nginx-proxy.yaml](apps/nginx-proxy.yaml) - Reverse proxy routing

### Useful Commands for Learning

```powershell
# View all resources
kubectl get all -n microservices-dev

# View pods with details
kubectl get pods -n microservices-dev -o wide

# Describe a resource (see events and config)
kubectl describe pod <pod-name> -n microservices-dev

# View logs (follow in real-time)
kubectl logs -f deployment/products-app -n microservices-dev

# Execute commands in pod
kubectl exec -it deployment/mongodb -n microservices-dev -- mongosh

# Connect to Redis
kubectl exec -it deployment/redis -n microservices-dev -- redis-cli

# Port forward for local testing
kubectl port-forward svc/nginx-proxy 8080:80 -n microservices-dev
# Then access: http://localhost:8080

# View resource usage
kubectl top nodes
kubectl top pods -n microservices-dev

# Edit a resource on the fly
kubectl edit deployment products-app -n microservices-dev

# Scale a deployment
kubectl scale deployment products-app --replicas=2 -n microservices-dev

# Restart a deployment
kubectl rollout restart deployment/products-app -n microservices-dev

# View rollout history
kubectl rollout history deployment/products-app -n microservices-dev

# View events (troubleshooting)
kubectl get events -n microservices-dev --sort-by='.lastTimestamp'
```

### MongoDB Commands

```powershell
# Connect to MongoDB shell
kubectl exec -it deployment/mongodb -n microservices-dev -- mongosh

# Inside mongosh:
> show dbs                          # List databases
> use microservices                 # Switch to database
> show collections                  # List collections
> db.products.find()                # Query products
> db.products.insertOne({name: "Test Product", price: 99.99})
> db.products.countDocuments()      # Count documents
> db.stats()                        # Database statistics
```

### Redis Commands

```powershell
# Connect to Redis CLI
kubectl exec -it deployment/redis -n microservices-dev -- redis-cli

# Inside redis-cli:
> PING                              # Test connection (returns PONG)
> KEYS *                            # List all keys
> SET mykey "Hello"                 # Set a key
> GET mykey                         # Get a key
> INFO memory                       # Memory usage
> INFO stats                        # Statistics
> MONITOR                           # Watch all commands in real-time
> DBSIZE                            # Number of keys
```

## 🔍 Monitoring and Troubleshooting

### Check Deployment Status

```powershell
# Overall status
kubectl get all -n microservices-dev

# Pod status
kubectl get pods -n microservices-dev

# Service endpoints
kubectl get svc -n microservices-dev

# Ingress status
kubectl get ingress -n microservices-dev

# PVC status
kubectl get pvc -n microservices-dev
```

### Common Issues and Solutions

#### Pods Stuck in Pending
```powershell
# Check pod events
kubectl describe pod <pod-name> -n microservices-dev

# Check node resources
kubectl top nodes
kubectl describe nodes

# Likely causes:
# - Insufficient CPU/memory on nodes
# - PVC not binding (EBS CSI driver issue)
```

#### Pods in CrashLoopBackOff
```powershell
# View logs
kubectl logs <pod-name> -n microservices-dev
kubectl logs <pod-name> -n microservices-dev --previous  # Previous crash

# Common causes:
# - Database connection issues
# - Missing environment variables
# - Out of memory
```

#### Cannot Access Load Balancer
```powershell
# Check Ingress status
kubectl describe ingress microservices-ingress -n microservices-dev

# Check AWS Load Balancer Controller
kubectl logs -n kube-system deployment/aws-load-balancer-controller

# Verify ALB exists in AWS Console
# EC2 > Load Balancers
```

#### 502 Bad Gateway
```powershell
# Backend service is down
kubectl get pods -n microservices-dev

# Check service endpoints
kubectl get endpoints -n microservices-dev

# Test service directly
kubectl exec deployment/nginx-proxy -n microservices-dev -- curl http://products-app:8080/health
```

## 🧹 Cleanup (IMPORTANT!)

### Quick Cleanup
```powershell
cd eks-deployment-dev
.\cleanup.ps1
```

### Complete Cleanup (Stop All AWS Charges)

```powershell
# 1. Delete application resources
.\cleanup.ps1

# 2. Delete node group
eksctl delete nodegroup `
  --cluster=microservices-learning `
  --name=dev-nodes `
  --region=ap-south-1 `
  --drain=false

# 3. Delete cluster
eksctl delete cluster `
  --name=microservices-learning `
  --region=ap-south-1
```

**⚠️ VERY IMPORTANT:**
- Run cleanup immediately after learning
- Verify in AWS Console that resources are deleted:
  - EC2 > Load Balancers (should be deleted)
  - EC2 > Volumes (should be deleted)
  - EKS > Clusters (should be deleted after full cleanup)

## 📊 Resource Limits

| Service | CPU Request | CPU Limit | Memory Request | Memory Limit |
|---------|-------------|-----------|----------------|--------------|
| MongoDB | 200m | 500m | 256Mi | 512Mi |
| Redis | 100m | 250m | 128Mi | 256Mi |
| Products | 100m | 250m | 128Mi | 256Mi |
| Users | 100m | 250m | 128Mi | 256Mi |
| Orders | 100m | 250m | 128Mi | 256Mi |
| React | 50m | 100m | 64Mi | 128Mi |
| Nginx | 50m | 100m | 32Mi | 64Mi |
| **Total** | **600m** | **1450m** | **760Mi** | **1792Mi** |

**Analysis:**
- Total requests: 0.6 CPU cores, 760 MB RAM
- Fits comfortably on 2× t3.small nodes (4 vCPU, 4 GB RAM total)
- Leaves room for system pods and overhead
- Can handle light-to-moderate learning traffic

## 🎓 Learning Path

### Beginner (Day 1-2)
1. Deploy the application
2. Explore pods, services, and deployments
3. View logs and understand basic troubleshooting
4. Scale deployments up and down
5. Connect to MongoDB and Redis

### Intermediate (Day 3-5)
1. Modify ConfigMaps and see updates
2. Update resource limits
3. Understand Ingress and Load Balancer
4. Implement rolling updates
5. Practice debugging failed deployments

### Advanced (Week 2)
1. Add health check endpoints
2. Implement readiness and liveness probes
3. Configure horizontal pod autoscaling
4. Add persistent volume claims
5. Implement secrets management

## 📝 Project Structure

```
eks-deployment-dev/
├── base/
│   ├── namespace.yaml           # Namespace definition
│   ├── storage.yaml             # Minimal PVCs (2GB+1GB)
│   └── configmap.yaml           # Application configuration
├── infrastructure/
│   ├── mongodb.yaml             # MongoDB (single replica, minimal resources)
│   └── redis.yaml               # Redis (single replica, minimal resources)
├── apps/
│   ├── products-app.yaml        # Products microservice
│   ├── users-app.yaml           # Users microservice
│   ├── orders-app.yaml          # Orders microservice
│   ├── react-app.yaml           # React frontend
│   ├── nginx-proxy.yaml         # Nginx reverse proxy
│   └── ingress.yaml             # AWS ALB configuration
├── deploy-all.ps1               # Automated deployment
├── cleanup.ps1                  # Automated cleanup
└── README.md                    # This file
```

## 🔗 Additional Resources

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS EKS Best Practices](https://aws.github.io/aws-eks-best-practices/)
- [kubectl Cheat Sheet](https://kubernetes.io/docs/reference/kubectl/cheatsheet/)
- [EKS Workshop](https://www.eksworkshop.com/)
- [Parent Project README](../README.md)

## ❓ FAQ

**Q: Why no Elasticsearch?**  
A: Elasticsearch requires significant resources (~1 GB RAM minimum). For learning, use `kubectl logs` instead.

**Q: Can I run this for longer periods?**  
A: Yes. For extended learning, consider stopping the cluster when not in use and restarting when needed.

**Q: Why single replicas?**  
A: High availability isn't needed for learning. Single replicas simplify the setup.

**Q: Can I increase resources if needed?**  
A: Yes! Edit the YAML files and increase `requests` and `limits`, then apply changes with `kubectl apply -f <file>`.

**Q: How do I cleanup after learning?**  
A: Delete everything after each session with `.\cleanup.ps1` and delete the cluster. Recreate when you need it again.

---

**Happy Learning! 🚀**

*For production deployments, see [../eks-deployment/README.md](../eks-deployment/README.md)*
