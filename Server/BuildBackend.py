import sys
import os

if __name__ == "__main__":
    if len(sys.argv) < 2:
        print("Usage: python3 BuildBackend.py <tag for backend>")
        exit(1)
    
    tag = sys.argv[1]
    os.system("docker buildx build --no-cache -t " + tag + ":latest -f ./Dockerfile-Backend . ")