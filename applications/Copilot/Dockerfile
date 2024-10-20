# syntax=docker/dockerfile:1

FROM python:3.10-slim-bookworm

EXPOSE 8080 

WORKDIR /
COPY . .

RUN pip install -r requirements.txt

# Should be able to add the root to the path (or .. after workdir set) have python get acess 
#  to the global modules from the root such as /shared, but doesn't work 
#  so using os.path.append trick in app.py
# ENV PATH=/:$PATH
# RUN ECHO $PATH (looks good from here)
WORKDIR /CopilotPythonHost

CMD ["python", "app.py"]
