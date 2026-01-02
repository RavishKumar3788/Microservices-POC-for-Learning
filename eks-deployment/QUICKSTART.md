# Quick Start Guide - EKS Deployment

This is a quick reference guide to get your microservices application running on AWS EKS.

## Prerequisites Checklist

- [ ] AWS Account with appropriate permissions
- [ ] AWS CLI installed and configured (`aws configure`)
- [ ] kubectl installed (v1.28+)
- [ ] eksctl installed
- [ ] Helm installed (v3+)

## Quick Deployment (30-45 minutes)

### 1. Create EKS Cluster (15-20 minutes)

```powershell
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

### 2. Install AWS Load Balancer Controller (5 minutes)

```powershell
# Associate OIDC provider
eksctl utils associate-iam-oidc-provider `
  --region ap-south-1 `
  --cluster microservices-eks `
  --approve

# Download IAM policy
Invoke-WebRequest -Uri https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/v2.6.0/docs/install/iam_policy.json -OutFile iam_policy.json

# Create IAM policy (replace <ACCOUNT_ID> with your AWS account ID)
aws iam create-policy `
  --policy-name AWSLoadBalancerControllerIAMPolicy `
  --policy-document file://iam_policy.json

# Create service account (replace <ACCOUNT_ID>)
eksctl create iamserviceaccount `
  --cluster=microservices-eks `
  --namespace=kube-system `
  --name=aws-load-balancer-controller `
  --attach-policy-arn=arn:aws:iam::<ACCOUNT_ID>:policy/AWSLoadBalancerControllerIAMPolicy `
  --override-existing-serviceaccounts `
  --region ap-south-1 `
  --approve

# Install using Helm
helm repo add eks https://aws.github.io/eks-charts
helm repo update

helm install aws-load-balancer-controller eks/aws-load-balancer-controller `
  -n kube-system `
  --set clusterName=microservices-eks `
  --set serviceAccount.create=false `
  --set serviceAccount.name=aws-load-balancer-controller
```

### 3. Install EBS CSI Driver (5 minutes)

```powershell
# Create IAM service account (replace <ACCOUNT_ID>)
eksctl create iamserviceaccount `
  --name ebs-csi-controller-sa `
  --namespace kube-system `
  --cluster microservices-eks `
  --region ap-south-1 `
  --attach-policy-arn arn:aws:iam::aws:policy/service-role/AmazonEBSCSIDriverPolicy `
  --approve `
  --role-only `
  --role-name AmazonEKS_EBS_CSI_DriverRole

# Install addon (replace <ACCOUNT_ID>)
aws eks create-addon `
  --cluster-name microservices-eks `
  --addon-name aws-ebs-csi-driver `
  --service-account-role-arn arn:aws:iam::<ACCOUNT_ID>:role/AmazonEKS_EBS_CSI_DriverRole `
  --region ap-south-1
```

### 4. Deploy Application (5-10 minutes)

```powershell
# Update kubeconfig
aws eks update-kubeconfig --region ap-south-1 --name microservices-eks

# Navigate to deployment folder
cd eks-deployment

# Deploy all services
.\deploy-all.ps1 -WaitForReady
```

### 5. Access Application

```powershell
# Get Load Balancer URL
kubectl get ingress microservices-ingress -n microservices-poc

# Access application at the ALB URL shown
# Example: http://k8s-microser-microser-xxxxx.ap-south-1.elb.amazonaws.com
```

## Verify Deployment

```powershell
# Check all pods are running
kubectl get pods -n microservices-poc

# Check services
kubectl get svc -n microservices-poc

# Check ingress
kubectl get ingress -n microservices-poc
```

## Common Issues

### Pods in Pending State
```powershell
# Check node capacity
kubectl describe nodes
kubectl top nodes
```

### Load Balancer Not Provisioning
```powershell
# Check controller logs
kubectl logs -n kube-system deployment/aws-load-balancer-controller
```

### PVCs Not Binding
```powershell
# Check EBS CSI driver
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-ebs-csi-driver
```

## Cleanup

```powershell
# Delete application resources
.\cleanup.ps1

# Delete EKS cluster
eksctl delete cluster --name microservices-eks --region ap-south-1
```

## Next Steps

- Configure SSL/TLS with ACM certificate
- Set up monitoring with Prometheus/Grafana
- Configure CI/CD pipeline
- Implement network policies
- Set up backup strategy

---

For detailed information, see [README.md](README.md)
