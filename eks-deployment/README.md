# Microservices Deployment on AWS EKS

Complete Kubernetes manifests and deployment scripts for deploying the microservices application on Amazon Elastic Kubernetes Service (EKS).

## 📋 Table of Contents

- [Architecture Overview](#architecture-overview)
- [Prerequisites](#prerequisites)
- [Project Structure](#project-structure)
- [Setup Instructions](#setup-instructions)
- [Deployment](#deployment)
- [Configuration](#configuration)
- [Monitoring and Troubleshooting](#monitoring-and-troubleshooting)
- [Cleanup](#cleanup)
- [Cost Optimization](#cost-optimization)

## 🏗️ Architecture Overview

This deployment includes:

### **Application Tier**
- **React Frontend** - SPA with Nginx
- **Products API** - .NET microservice with MongoDB + Redis
- **Users API** - .NET microservice with MongoDB + Redis
- **Orders API** - .NET microservice with MongoDB + Redis
- **Nginx Proxy** - API Gateway and reverse proxy

### **Infrastructure Tier**
- **MongoDB** - Primary database with persistent storage
- **Redis** - Distributed cache
- **Elasticsearch** - Centralized logging and search

### **AWS Resources**
- **EKS Cluster** - Managed Kubernetes cluster
- **Application Load Balancer (ALB)** - Internet-facing ingress
- **EBS Volumes** - Persistent storage for databases
- **VPC & Subnets** - Network isolation
- **IAM Roles** - Service account permissions

## ✅ Prerequisites

### Required Software
- **kubectl** (v1.28+) - [Install Guide](https://kubernetes.io/docs/tasks/tools/)
- **AWS CLI** (v2) - [Install Guide](https://aws.amazon.com/cli/)
- **eksctl** - [Install Guide](https://eksctl.io/installation/)
- **Helm** (v3) - [Install Guide](https://helm.sh/docs/intro/install/)

### AWS Account Requirements
- AWS account with appropriate permissions
- IAM permissions to create EKS clusters, EC2 instances, Load Balancers, EBS volumes
- Configured AWS credentials (`aws configure`)

### Verify Installation
```powershell
# Check kubectl
kubectl version --client

# Check AWS CLI
aws --version

# Check eksctl
eksctl version

# Check Helm
helm version

# Verify AWS credentials
aws sts get-caller-identity
```

## 📁 Project Structure

```
eks-deployment/
├── base/
│   ├── namespace.yaml          # Namespace definition
│   ├── storage.yaml            # PVCs with EBS storage class
│   └── configmap.yaml          # Application configuration
├── infrastructure/
│   ├── mongodb.yaml            # MongoDB deployment
│   ├── redis.yaml              # Redis deployment
│   └── elasticsearch.yaml      # Elasticsearch deployment
├── apps/
│   ├── products-app.yaml       # Products microservice
│   ├── users-app.yaml          # Users microservice
│   ├── orders-app.yaml         # Orders microservice
│   ├── react-app.yaml          # React frontend
│   ├── nginx-proxy.yaml        # Nginx reverse proxy
│   └── ingress.yaml            # AWS ALB Ingress
├── deploy-all.ps1              # Automated deployment script
├── cleanup.ps1                 # Cleanup script
└── README.md                   # This file
```

## 🚀 Setup Instructions

### Step 1: Create EKS Cluster

Create an EKS cluster with the recommended configuration:

```powershell
# Create cluster with eksctl (takes 15-20 minutes)
eksctl create cluster `
  --name microservices-eks `
  --region ap-south-1 `
  --nodegroup-name standard-workers `
  --node-type t3.medium `
  --nodes 3 `
  --nodes-min 2 `
  --nodes-max 5 `
  --managed
```

**Cluster Configuration:**
- **Instance Type:** t3.medium (2 vCPU, 4GB RAM)
- **Node Count:** 3 nodes (min: 2, max: 5)
- **Region:** ap-south-1 (change as needed)
- **Managed Node Group:** Auto-scaling enabled

### Step 2: Install AWS Load Balancer Controller

The ALB Ingress requires the AWS Load Balancer Controller:

```powershell
# Create IAM OIDC provider for the cluster
eksctl utils associate-iam-oidc-provider `
  --region ap-south-1 `
  --cluster microservices-eks `
  --approve

# Download IAM policy
Invoke-WebRequest -Uri https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/v2.6.0/docs/install/iam_policy.json -OutFile iam_policy.json

# Create IAM policy
aws iam create-policy `
  --policy-name AWSLoadBalancerControllerIAMPolicy `
  --policy-document file://iam_policy.json

# Create service account
eksctl create iamserviceaccount `
  --cluster=microservices-eks `
  --namespace=kube-system `
  --name=aws-load-balancer-controller `
  --attach-policy-arn=arn:aws:iam::<ACCOUNT_ID>:policy/AWSLoadBalancerControllerIAMPolicy `
  --override-existing-serviceaccounts `
  --region ap-south-1 `
  --approve

# Install AWS Load Balancer Controller using Helm
helm repo add eks https://aws.github.io/eks-charts
helm repo update

helm install aws-load-balancer-controller eks/aws-load-balancer-controller `
  -n kube-system `
  --set clusterName=microservices-eks `
  --set serviceAccount.create=false `
  --set serviceAccount.name=aws-load-balancer-controller

# Verify installation
kubectl get deployment -n kube-system aws-load-balancer-controller
```

### Step 3: Install EBS CSI Driver

Required for persistent storage:

```powershell
# Create IAM service account for EBS CSI driver
eksctl create iamserviceaccount `
  --name ebs-csi-controller-sa `
  --namespace kube-system `
  --cluster microservices-eks `
  --region ap-south-1 `
  --attach-policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy `
  --approve `
  --role-only `
  --role-name AmazonEKS_EBS_CSI_DriverRole

# Install EBS CSI driver addon
aws eks create-addon `
  --cluster-name microservices-eks `
  --addon-name aws-ebs-csi-driver `
  --service-account-role-arn arn:aws:iam::<ACCOUNT_ID>:role/AmazonEKS_EBS_CSI_DriverRole `
  --region ap-south-1

# Verify installation
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-ebs-csi-driver
```

### Step 4: Configure kubectl Context

```powershell
# Update kubeconfig
aws eks update-kubeconfig --region ap-south-1 --name microservices-eks

# Verify connection
kubectl cluster-info
kubectl get nodes
```

## 📦 Deployment

### Option 1: Automated Deployment (Recommended)

Use the PowerShell deployment script:

```powershell
# Navigate to eks-deployment folder
cd eks-deployment

# Deploy all resources
.\deploy-all.ps1

# Deploy with waiting for pods to be ready
.\deploy-all.ps1 -WaitForReady

# Deploy applications only (skip infrastructure)
.\deploy-all.ps1 -SkipInfrastructure
```

The script will:
1. Create namespace
2. Create ConfigMaps
3. Create storage resources
4. Deploy infrastructure (MongoDB, Redis, Elasticsearch)
5. Deploy applications (Products, Users, Orders, React)
6. Deploy Nginx proxy
7. Create AWS Load Balancer Ingress
8. Display deployment status and URLs

### Option 2: Manual Step-by-Step Deployment

```powershell
# 1. Create namespace
kubectl apply -f base/namespace.yaml

# 2. Create ConfigMaps
kubectl apply -f base/configmap.yaml

# 3. Create storage
kubectl apply -f base/storage.yaml

# 4. Deploy infrastructure (wait between each for readiness)
kubectl apply -f infrastructure/mongodb.yaml
kubectl apply -f infrastructure/redis.yaml
kubectl apply -f infrastructure/elasticsearch.yaml

# Wait for infrastructure
kubectl wait --for=condition=ready pod -l app=mongodb -n microservices-poc --timeout=300s
kubectl wait --for=condition=ready pod -l app=redis -n microservices-poc --timeout=180s
kubectl wait --for=condition=ready pod -l app=elasticsearch -n microservices-poc --timeout=300s

# 5. Deploy applications
kubectl apply -f apps/products-app.yaml
kubectl apply -f apps/users-app.yaml
kubectl apply -f apps/orders-app.yaml
kubectl apply -f apps/react-app.yaml

# 6. Deploy Nginx proxy
kubectl apply -f apps/nginx-proxy.yaml

# 7. Create Ingress (AWS Load Balancer)
kubectl apply -f apps/ingress.yaml
```

### Verify Deployment

```powershell
# Check all resources
kubectl get all -n microservices-poc

# Check pods status
kubectl get pods -n microservices-poc

# Check services
kubectl get svc -n microservices-poc

# Check ingress and Load Balancer
kubectl get ingress -n microservices-poc

# Get Load Balancer URL
kubectl get ingress microservices-ingress -n microservices-poc -o jsonpath='{.status.loadBalancer.ingress[0].hostname}'
```

## ⚙️ Configuration

### Environment Variables

Modify [base/configmap.yaml](base/configmap.yaml) to update configuration:

```yaml
data:
  MONGODB_CONNECTION_STRING: "mongodb://mongodb:27017"
  REDIS_CONNECTION_STRING: "redis:6379"
  ELASTICSEARCH_URL: "http://elasticsearch:9200"
  ASPNETCORE_ENVIRONMENT: "Production"
```

### Resource Limits

Adjust resource requests/limits in deployment files:

```yaml
resources:
  requests:
    memory: "256Mi"
    cpu: "250m"
  limits:
    memory: "512Mi"
    cpu: "500m"
```

### Auto-Scaling

Horizontal Pod Autoscaler (HPA) is configured for all services:

```powershell
# View HPA status
kubectl get hpa -n microservices-poc

# Adjust scaling parameters (edit the yaml files)
# - minReplicas: Minimum number of pods
# - maxReplicas: Maximum number of pods
# - CPU/Memory thresholds
```

### SSL/TLS Configuration

To enable HTTPS with AWS Certificate Manager (ACM):

1. Create/import certificate in ACM
2. Update [apps/ingress.yaml](apps/ingress.yaml):

```yaml
annotations:
  alb.ingress.kubernetes.io/certificate-arn: arn:aws:acm:region:account-id:certificate/cert-id
  alb.ingress.kubernetes.io/listen-ports: '[{"HTTP": 80}, {"HTTPS": 443}]'
  alb.ingress.kubernetes.io/ssl-redirect: '443'
```

## 📊 Monitoring and Troubleshooting

### View Logs

```powershell
# Application logs
kubectl logs -f deployment/products-app -n microservices-poc
kubectl logs -f deployment/users-app -n microservices-poc
kubectl logs -f deployment/orders-app -n microservices-poc

# Infrastructure logs
kubectl logs -f deployment/mongodb -n microservices-poc
kubectl logs -f deployment/redis -n microservices-poc
kubectl logs -f deployment/elasticsearch -n microservices-poc

# Nginx proxy logs
kubectl logs -f deployment/nginx-proxy -n microservices-poc

# View logs from all replicas
kubectl logs -l app=products-app -n microservices-poc --all-containers=true
```

### Debug Pods

```powershell
# Describe pod for events
kubectl describe pod <pod-name> -n microservices-poc

# Get pod details
kubectl get pod <pod-name> -n microservices-poc -o yaml

# Execute commands in pod
kubectl exec -it <pod-name> -n microservices-poc -- /bin/sh

# Port forward for local testing
kubectl port-forward svc/products-app 8080:8080 -n microservices-poc
```

### Common Issues

**Pods stuck in Pending:**
```powershell
# Check events
kubectl get events -n microservices-poc --sort-by='.lastTimestamp'

# Check node resources
kubectl top nodes
kubectl describe nodes
```

**Load Balancer not provisioning:**
```powershell
# Check AWS Load Balancer Controller logs
kubectl logs -n kube-system deployment/aws-load-balancer-controller

# Verify controller is running
kubectl get deployment -n kube-system aws-load-balancer-controller
```

**Persistent Volume Claims not binding:**
```powershell
# Check PVC status
kubectl get pvc -n microservices-poc

# Check storage class
kubectl get storageclass

# Verify EBS CSI driver
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-ebs-csi-driver
```

## 🧹 Cleanup

### Option 1: Automated Cleanup (Recommended)

```powershell
# Delete all resources
.\cleanup.ps1

# Keep namespace
.\cleanup.ps1 -KeepNamespace

# Force cleanup without confirmation
.\cleanup.ps1 -Force
```

### Option 2: Manual Cleanup

```powershell
# Delete in reverse order
kubectl delete -f apps/ingress.yaml
kubectl delete -f apps/nginx-proxy.yaml
kubectl delete -f apps/react-app.yaml
kubectl delete -f apps/orders-app.yaml
kubectl delete -f apps/users-app.yaml
kubectl delete -f apps/products-app.yaml
kubectl delete -f infrastructure/elasticsearch.yaml
kubectl delete -f infrastructure/redis.yaml
kubectl delete -f infrastructure/mongodb.yaml
kubectl delete -f base/storage.yaml
kubectl delete -f base/configmap.yaml
kubectl delete -f base/namespace.yaml
```

### Delete EKS Cluster

```powershell
# Delete cluster (this will delete all resources)
eksctl delete cluster --name microservices-eks --region ap-south-1

# Verify deletion
aws eks list-clusters --region ap-south-1
```

### Verify AWS Resources Deleted

Check AWS Console for:
- **EC2**: Load Balancers should be deleted
- **EC2**: EBS volumes should be deleted
- **VPC**: Ensure no orphaned resources

##  Additional Resources

- [EKS Documentation](https://docs.aws.amazon.com/eks/)
- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [AWS Load Balancer Controller](https://kubernetes-sigs.github.io/aws-load-balancer-controller/)
- [EBS CSI Driver](https://github.com/kubernetes-sigs/aws-ebs-csi-driver)
- [eksctl Documentation](https://eksctl.io/)

## 🆘 Support

For issues or questions:
1. Check [TROUBLESHOOTING.md](../k8s-learning/TROUBLESHOOTING.md)
2. Review logs using commands above
3. Check AWS CloudWatch for infrastructure issues
4. Verify security groups and IAM permissions

## 📝 Notes

- **Load Balancer provisioning** takes 2-3 minutes after applying ingress
- **Elasticsearch** requires init containers for proper permissions
- **Auto-scaling** requires metrics-server (usually pre-installed on EKS)
- **Pod Security Standards** are enforced - ensure compliance
- **Network Policies** can be added for enhanced security

---

**Last Updated:** December 2025  
**EKS Version:** 1.28+  
**Kubernetes Version:** 1.28+
