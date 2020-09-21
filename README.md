# DockerNetSample

Example of producer/consumer system, written in .NET Core 3.1 with deployment to Docker, docker-compose and to Kubernetes (k8s). The sample contains different types of deployments (blue-green, canary) to k8s.

## Repo Structure

* *LoaderClient* - console application gRpc client which sends requests to API to produce continuos load
* *WorkerService* - gRpc service which performs simple CPU-bound task
* *StatsDServer* - project to keep deployments of StatsD server and Grafana

## Content

* Loader-Worker system sample - _DONE_
* StatsD and Grafana containerization - _DONE_
* .NET Core containerization
* Blue-Green deployments
* Canary deployments

## Articles

* [Deployment to Docker and Kubernetes of StatsD-Grafana bundle](https://medium.com/@alexander.goida/deployment-to-docker-and-kubernetes-of-statsd-grafana-bundle-543d776a29b6)
