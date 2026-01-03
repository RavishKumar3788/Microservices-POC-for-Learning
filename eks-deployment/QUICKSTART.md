# Quick Start Guide - EKS Deployment

So you want to get this thing running on AWS EKS? Cool! This guide will get you up and running in about 30-45 minutes. I've tried to keep it as straightforward as possible.

## What You'll Need

Before we start, make sure you've got these installed:

- [ ] AWS Account (with permissions to create EKS clusters and other resources)
- [ ] AWS CLI configured - run `aws configure` if you haven't already
- [ ] kubectl (v1.28 or newer)
- [ ] eksctl - trust me, this makes life so much easier
- [ ] Helm (v3+)

## Let's Get Started

### 1. Spin Up Your EKS Cluster (grab a coffee, this takes ~15-20 minutes)

First up, we need to create the cluster. This command will set up a nice little EKS cluster with 3 nodes:

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

Go ahead and run this. Seriously, go grab that coffee - AWS takes its time provisioning everything.

### 2. Install the AWS Load Balancer Controller

Alright, cluster's up! Now we need the Load Balancer Controller so our ingress can actually create an ALB. There are a few steps here, but just copy-paste and you'll be fine:

```powershell
# First, associate the OIDC provider (this lets EKS talk to IAM)
eksctl utils associate-iam-oidc-provider `
  --region ap-south-1 `
  --cluster microservices-eks `
  --approve

# Download the IAM policy
Invoke-WebRequest -Uri https://raw.githubusercontent.com/kubernetes-sigs/aws-load-balancer-controller/v2.6.0/docs/install/iam_policy.json -OutFile iam_policy.json

# Create the IAM policy
# Note: Replace <ACCOUNT_ID> with your actual AWS account ID (run 'aws sts get-caller-identity' to get it)
aws iam create-policy `
  --policy-name AWSLoadBalancerControllerIAMPolicy `
  --policy-document file://iam_policy.json

# Now create the service account (again, replace <ACCOUNT_ID>)
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

### 4. Deploy the Application

OK, infrastructure's ready. Time to deploy the actual app!

```powershell
# First, make sure kubectl knows about your cluster
aws eks update-kubeconfig --region ap-south-1 --name microservices-eks

# Jump into the deployment folder
cd eks-deployment

# Fire it up! The -WaitForReady flag will wait for everything to be ready before finishing
.\deploy-all.ps1 -WaitForReady
```

This will deploy all the microservices, databases, and everything else. Takes about 5-10 minutes.

### 5. Check It Out!

Everything should be running now. Let's get the URL:

```powershell
# Grab the Load Balancer URL
kubectl get ingress microservices-ingress -n microservices-poc

# You'll see something like: http://k8s-microser-microser-xxxxx.ap-south-1.elb.amazonaws.com
# Copy that URL and open it in your browser!
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

## If Things Go Wrong

Here are some issues I've run into (and how to fix them):

### Pods Stuck in "Pending"
Usually means your nodes don't have enough capacity. Check what's going on:
```powershell
kubectl describe nodes
kubectl top nodes
```
If nodes are maxed out, you might need bigger instances or more nodes.

### Load Balancer Never Shows Up
The ALB controller might be having issues. Check its logs:
```powershell
kubectl logs -n kube-system deployment/aws-load-balancer-controller
```
Often it's an IAM permissions thing - double-check you replaced all the <ACCOUNT_ID> placeholders!

### PVCs Not Binding
```powershell
# Check EBS CSI driver
kubectl get pods -n kube-system -l app.kubernetes.io/name=aws-ebs-csi-driver
```

## Tearing It Down

When you're done playing around (or if you want to avoid AWS charges), here's how to clean up:

```powershell
# Remove all the app stuff
.\cleanup.ps1

# Nuke the entire cluster (this takes a few minutes too)
eksctl delete cluster --name microservices-eks --region ap-south-1
```

**Pro tip:** Don't forget to do this! EKS clusters aren't free, and I've definitely forgotten to delete test clusters before. Whoops.

## What's Next?

Once you've got everything running, here are some cool things to try:

- **Add SSL/TLS** - Get a cert from ACM and enable HTTPS (your users will thank you)
- **Set up monitoring** - Prometheus and Grafana make it easy to see what's going on
- **Build a CI/CD pipeline** - Automate your deployments with GitHub Actions or similar
- **Lock down networking** - Network policies can prevent services from talking to things they shouldn't
- **Backup your data** - Because losing data sucks

Want more details? Check out the [full README](README.md) - it's got way more info.

---

Questions? Issues? Something I missed? Feel free to reach out or open an issue!
