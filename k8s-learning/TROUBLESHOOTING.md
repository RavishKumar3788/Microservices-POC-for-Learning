# Troubleshooting Guide

This guide helps you solve common issues when deploying to Kubernetes.

## Table of Contents
1. [Pod Issues](#pod-issues)
2. [Image Issues](#image-issues)
3. [Storage Issues](#storage-issues)
4. [Network Issues](#network-issues)
5. [Resource Issues](#resource-issues)
6. [Application Issues](#application-issues)

---

## Pod Issues

### Pod Stuck in Pending State

**Symptom**: Pod shows "Pending" status for a long time

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc
```

**Common Causes**:

1. **Insufficient Resources**
   - Look for: "Insufficient cpu" or "Insufficient memory" in events
   - Solution: Reduce resource requests or add more nodes
   ```powershell
   kubectl describe node
   ```

2. **PVC Not Bound**
   - Look for: "persistentvolumeclaim not found" or "unbound"
   - Solution: Check if PVC exists and is bound
   ```powershell
   kubectl get pvc -n microservices-poc
   ```

3. **Node Selector Issues**
   - Solution: Check if any node matches the selector

### Pod in CrashLoopBackOff

**Symptom**: Pod keeps restarting

**Check Logs**:
```powershell
# Current logs
kubectl logs <pod-name> -n microservices-poc

# Previous container logs
kubectl logs <pod-name> --previous -n microservices-poc
```

**Common Causes**:

1. **Application Crashes**
   - Check logs for error messages
   - Fix application code

2. **Wrong Environment Variables**
   - Check env vars in deployment
   ```powershell
   kubectl describe pod <pod-name> -n microservices-poc
   ```

3. **Failed Health Checks**
   - Health check endpoint not working
   - Solution: Check if `/health` endpoint exists
   ```powershell
   kubectl port-forward <pod-name> 8080:8080 -n microservices-poc
   curl http://localhost:8080/health
   ```

4. **Database Not Ready**
   - App tries to connect before DB is ready
   - Solution: Wait for databases
   ```powershell
   kubectl get pods -l app=mongodb -n microservices-poc
   ```

---

## Image Issues

### ImagePullBackOff

**Symptom**: Can't pull Docker image

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc
```

**Common Causes**:

1. **Image Doesn't Exist**
   - Check if image is on Docker Hub
   ```powershell
   docker pull ravishchauhan/products-app:latest
   ```
   - Solution: Push images first
   ```powershell
   cd ..
   docker compose push
   ```

2. **Wrong Image Name**
   - Check image name in YAML
   - Should be: `ravishchauhan/products-app:latest`
   - Solution: Fix image name and reapply
   ```powershell
   kubectl apply -f step8-products-app.yaml
   ```

3. **Private Registry Authentication**
   - Need credentials for private registry
   - Solution: Create image pull secret (not needed for public Docker Hub)

### Wrong Image Version

**Symptom**: Old version of app is running

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc | Select-String "Image:"
```

**Solution**:
```powershell
# Update image
kubectl set image deployment/products-app products-app=ravishchauhan/products-app:v2 -n microservices-poc

# Or use imagePullPolicy: Always
```

---

## Storage Issues

### PVC Stuck in Pending

**Symptom**: PersistentVolumeClaim not binding

**Check**:
```powershell
kubectl describe pvc <pvc-name> -n microservices-poc
kubectl get pv
kubectl get storageclass
```

**Common Causes**:

1. **No Default StorageClass**
   ```powershell
   # Check if default storage class exists
   kubectl get storageclass
   ```
   - Solution for Docker Desktop: It should have default storageclass
   - Solution for Minikube: 
   ```powershell
   minikube addons enable storage-provisioner
   ```

2. **Insufficient Storage**
   - Check if there's enough disk space on your machine

### Data Not Persisting

**Symptom**: Data lost after pod restart

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc
```

**Solution**:
- Verify volumeMounts and volumes are configured
- Check if PVC is mounted correctly

---

## Network Issues

### Can't Access Service

**Symptom**: Service not reachable

**Check**:
```powershell
kubectl get service -n microservices-poc
kubectl get endpoints -n microservices-poc
```

**Common Causes**:

1. **Service Selector Mismatch**
   - Service selector must match pod labels
   ```powershell
   kubectl get service products-app -n microservices-poc -o yaml
   kubectl get pods -l app=products-app -n microservices-poc
   ```

2. **Wrong Port**
   - Check service port vs container port
   ```powershell
   kubectl describe service products-app -n microservices-poc
   ```

3. **No Pods Ready**
   - Service needs at least one ready pod
   ```powershell
   kubectl get pods -l app=products-app -n microservices-poc
   ```

### DNS Not Working

**Symptom**: Services can't reach each other

**Test**:
```powershell
# Create debug pod
kubectl run debug --image=busybox -it --rm -n microservices-poc -- nslookup products-app
```

**Solution**:
- Use service name: `products-app`
- Full DNS name: `products-app.microservices-poc.svc.cluster.local`
- Check CoreDNS is running:
```powershell
kubectl get pods -n kube-system -l k8s-app=kube-dns
```

### Can't Access from Outside

**Symptom**: Can't reach http://localhost:30080

**Check**:
```powershell
kubectl get service nginx-proxy-external -n microservices-poc
```

**Solutions**:

1. **Docker Desktop**:
   - Access: http://localhost:30080
   - Make sure Kubernetes is enabled in Docker Desktop

2. **Minikube**:
   ```powershell
   # Get minikube IP
   minikube ip
   
   # Access: http://<minikube-ip>:30080
   # Or use tunnel:
   minikube service nginx-proxy-external -n microservices-poc
   ```

3. **Port Already in Use**:
   - Change nodePort to different port (30000-32767)

---

## Resource Issues

### Out of Memory

**Symptom**: Pod killed with OOMKilled

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc
```

**Solution**:
- Increase memory limit in deployment
- Or fix memory leak in application

### CPU Throttling

**Symptom**: App is slow

**Check**:
```powershell
kubectl top pods -n microservices-poc
```

**Solution**:
- Increase CPU limit
- Optimize application code

### Not Enough Resources

**Symptom**: Pods pending due to insufficient resources

**Check**:
```powershell
kubectl describe node
```

**Solution**:
- Reduce resource requests
- Add more nodes to cluster
- Delete unused resources

---

## Application Issues

### Application Not Starting

**Check Logs**:
```powershell
kubectl logs -f deployment/products-app -n microservices-poc
```

**Common Issues**:

1. **Database Connection Failed**
   - Check if MongoDB is running
   ```powershell
   kubectl get pods -l app=mongodb -n microservices-poc
   ```
   - Check connection string in app configuration

2. **Missing Environment Variables**
   - Add required env vars to deployment
   ```powershell
   kubectl edit deployment products-app -n microservices-poc
   ```

3. **Port Already in Use**
   - Each pod uses its own network namespace, shouldn't conflict
   - Check if port in container is correct

### Health Check Failing

**Symptom**: Pod restarts frequently

**Check**:
```powershell
kubectl describe pod <pod-name> -n microservices-poc
```

**Look for**: "Liveness probe failed" or "Readiness probe failed"

**Solution**:
1. Test health endpoint manually:
   ```powershell
   kubectl port-forward <pod-name> 8080:8080 -n microservices-poc
   curl http://localhost:8080/health
   ```

2. Increase `initialDelaySeconds` if app needs more time to start

3. Make sure `/health` endpoint exists in your app

### Logs Not Showing

**Issue**: `kubectl logs` shows nothing

**Possible Reasons**:
1. Application not logging to stdout
   - Solution: Configure app to log to console

2. Container not started yet
   - Wait for container to start

3. Wrong container name (if multiple containers)
   ```powershell
   kubectl logs <pod-name> -c <container-name> -n microservices-poc
   ```

---

## General Debugging Steps

### 1. Check Overall Status
```powershell
kubectl get all -n microservices-poc
```

### 2. Check Specific Resource
```powershell
kubectl describe pod <pod-name> -n microservices-poc
kubectl describe deployment <deployment-name> -n microservices-poc
kubectl describe service <service-name> -n microservices-poc
```

### 3. Check Events
```powershell
kubectl get events -n microservices-poc --sort-by='.lastTimestamp'
```

### 4. Check Logs
```powershell
kubectl logs -f <pod-name> -n microservices-poc
kubectl logs <pod-name> --previous -n microservices-poc
```

### 5. Execute Commands in Pod
```powershell
kubectl exec -it <pod-name> -n microservices-poc -- /bin/sh
```

### 6. Test Networking
```powershell
# DNS resolution
kubectl run debug --image=busybox -it --rm -n microservices-poc -- nslookup products-app

# HTTP test
kubectl run debug --image=curlimages/curl -it --rm -n microservices-poc -- curl http://products-app:8080/health
```

---

## Still Having Issues?

### Collect Information

1. **Get all resources**:
   ```powershell
   kubectl get all -n microservices-poc > resources.txt
   ```

2. **Get pod details**:
   ```powershell
   kubectl describe pod <pod-name> -n microservices-poc > pod-details.txt
   ```

3. **Get logs**:
   ```powershell
   kubectl logs <pod-name> -n microservices-poc > pod-logs.txt
   ```

4. **Get events**:
   ```powershell
   kubectl get events -n microservices-poc --sort-by='.lastTimestamp' > events.txt
   ```

### Start Fresh

If everything is broken, delete and redeploy:

```powershell
# Delete everything
kubectl delete namespace microservices-poc

# Wait a moment
Start-Sleep -Seconds 10

# Deploy again
.\deploy-all.ps1
```

---

## Getting Help

- [Kubernetes Documentation](https://kubernetes.io/docs/)
- [Kubernetes Slack](https://kubernetes.slack.com/)
- [Stack Overflow](https://stackoverflow.com/questions/tagged/kubernetes)
- Check logs carefully - they usually tell you what's wrong!
