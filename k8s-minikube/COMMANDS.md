# Kubernetes Commands You'll Actually Use

Here's your kubectl cheat sheet. I've organized this by what you're trying to do, not by alphabetical order (because who memorizes commands alphabetically?).

## Just Show Me Everything

### See What's Running
```powershell
# The "tell me everything" command
kubectl get all -n microservices-poc

# Or get specific about it
kubectl get pods -n microservices-poc
kubectl get deployments -n microservices-poc
kubectl get services -n microservices-poc
kubectl get pvc -n microservices-poc

# Watch things in real-time (updates automatically)
kubectl get pods -n microservices-poc -w
```

## When Something's Wrong with a Pod

### Figure Out What's Happening
```powershell
# Get the full story about a pod
kubectl describe pod <pod-name> -n microservices-poc

# Read the logs (this is usually where the answer is)
kubectl logs <pod-name> -n microservices-poc

# Stream logs live (like tail -f)
kubectl logs -f <pod-name> -n microservices-poc

# See logs from all pods in a deployment (super useful)
kubectl logs -f deployment/products-app -n microservices-poc

# Pod crashed? Check what happened before it died
kubectl logs <pod-name> --previous -n microservices-poc
```

### Get Inside a Pod
```powershell
# SSH-style access to a pod
kubectl exec -it <pod-name> -n microservices-poc -- /bin/sh

# Run a one-off command
kubectl exec <pod-name> -n microservices-poc -- ls /app

# Jump into MongoDB and poke around
kubectl exec -it deployment/mongodb -n microservices-poc -- mongosh
```

### Port Forwarding
```powershell
# Forward local port to service
kubectl port-forward -n microservices-poc service/nginx-proxy 8080:80

# Forward to specific pod
kubectl port-forward -n microservices-poc <pod-name> 8080:80

# Access multiple services
kubectl port-forward -n microservices-poc service/mongodb 27017:27017
kubectl port-forward -n microservices-poc service/redis 6379:6379
```

## Scaling and Updating

### Make More (or Fewer) Copies
```powershell
# Need more capacity? Scale it up!
kubectl scale deployment products-app --replicas=3 -n microservices-poc

# Scale multiple things at once
kubectl scale deployment products-app users-app orders-app --replicas=3 -n microservices-poc

# See how many you're running
kubectl get deployment products-app -n microservices-poc
```

### Deploy a New Version
```powershell
# Push out a new image version
kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc

# Watch it roll out (Kubernetes does this gradually)
kubectl rollout status deployment/products-app -n microservices-poc

# See the history of your deployments
kubectl rollout history deployment/products-app -n microservices-poc

# Oh crap, that version was bad - roll it back!
kubectl rollout undo deployment/products-app -n microservices-poc

# Go back to a specific version
kubectl rollout undo deployment/products-app --to-revision=2 -n microservices-poc
```

### Restart Deployment
```powershell
# Restart all pods in deployment (rolling restart)
kubectl rollout restart deployment/products-app -n microservices-poc
```

### Update Configuration
```powershell
# Edit deployment directly
kubectl edit deployment products-app -n microservices-poc

# Edit service
kubectl edit service products-app -n microservices-poc

# Update from file
kubectl apply -f step8-products-app.yaml
```

## Troubleshooting 101

### Pod Won't Start? Here's Your Checklist:
```powershell
# 1. What's the current status?
kubectl get pods -n microservices-poc

# 2. What does Kubernetes say is wrong?
kubectl describe pod <pod-name> -n microservices-poc

# 3. What do the app logs say?
kubectl logs <pod-name> -n microservices-poc

# 4. If it crashed and restarted, check the previous logs
kubectl logs <pod-name> --previous -n microservices-poc
```

### Understanding Pod States
- **Pending**: Waiting for resources or being scheduled (usually means "hang on a sec")
- **ContainerCreating**: Downloading the image (patience, grasshopper)
- **Running**: Everything's fine! Ship it!
- **CrashLoopBackOff**: Keeps dying and restarting (check those logs)
- **ImagePullBackOff**: Can't download the image (typo in the image name?)
- **Error**: Something went boom (again, check logs)

### Check Resource Usage
```powershell
# Pod resource usage (requires metrics-server)
kubectl top pods -n microservices-poc

# Node resource usage
kubectl top nodes

# Describe node to see allocated resources
kubectl describe node
```

### Network Testing
```powershell
# Create debug pod
kubectl run debug --image=busybox -it --rm -n microservices-poc -- /bin/sh

# Test DNS resolution
kubectl run debug --image=busybox -it --rm -n microservices-poc -- nslookup products-app

# Test connectivity
kubectl run debug --image=curlimages/curl -it --rm -n microservices-poc -- curl http://products-app:8080/health
```

### View Events
```powershell
# All events in namespace
kubectl get events -n microservices-poc --sort-by='.lastTimestamp'

# Events for specific pod
kubectl get events --field-selector involvedObject.name=<pod-name> -n microservices-poc
```

## ConfigMaps and Secrets

### View ConfigMaps
```powershell
# List ConfigMaps
kubectl get configmaps -n microservices-poc

# View ConfigMap content
kubectl describe configmap nginx-config -n microservices-poc

# Get ConfigMap as YAML
kubectl get configmap nginx-config -n microservices-poc -o yaml
```

### Update ConfigMap
```powershell
# Edit ConfigMap
kubectl edit configmap nginx-config -n microservices-poc

# Update from file
kubectl apply -f step2-configmaps.yaml

# Restart pods to pick up new config
kubectl rollout restart deployment/nginx-proxy -n microservices-poc
```

## Storage

### View Storage
```powershell
# PersistentVolumeClaims
kubectl get pvc -n microservices-poc

# PersistentVolumes
kubectl get pv

# Describe PVC
kubectl describe pvc mongodb-pvc -n microservices-poc
```

## Cleanup

### Delete Specific Resources
```powershell
# Delete deployment
kubectl delete deployment products-app -n microservices-poc

# Delete service
kubectl delete service products-app -n microservices-poc

# Delete from file
kubectl delete -f step8-products-app.yaml
```

### Delete Everything
```powershell
# Delete entire namespace (removes everything)
kubectl delete namespace microservices-poc

# Or run cleanup script
.\cleanup.ps1
```

## Time-Saving Tips

### Stop Typing the Namespace Every Time
```powershell
# Set a default namespace so you don't have to keep typing -n microservices-poc
kubectl config set-context --current --namespace=microservices-poc

# Now you can just do:
kubectl get pods
# Instead of:
kubectl get pods -n microservices-poc
```

### Different Ways to View Stuff
```powershell
# Get the raw JSON (good for scripts)
kubectl get pods -n microservices-poc -o json

# Get it as YAML (easier to read)
kubectl get pods -n microservices-poc -o yaml

# More details in table format
kubectl get pods -n microservices-poc -o wide

# Just show me what I care about
kubectl get pods -n microservices-poc -o custom-columns=NAME:.metadata.name,STATUS:.status.phase
```

### Use Labels Like a Pro
```powershell
# Show only pods with a specific label
kubectl get pods -l app=products-app -n microservices-poc
kubectl get pods -l tier=backend -n microservices-poc

# Get everything related to your backend
kubectl get all -l tier=backend -n microservices-poc

# Tag something with a label
kubectl label pod <pod-name> environment=production -n microservices-poc
```

### Copy Files
```powershell
# Copy file from pod to local
kubectl cp microservices-poc/<pod-name>:/path/to/file ./local-file

# Copy file from local to pod
kubectl cp ./local-file microservices-poc/<pod-name>:/path/to/file
```

## Learning Resources

- Official Docs: https://kubernetes.io/docs/
- kubectl Cheat Sheet: https://kubernetes.io/docs/reference/kubectl/cheatsheet/
- Interactive Tutorial: https://kubernetes.io/docs/tutorials/kubernetes-basics/
