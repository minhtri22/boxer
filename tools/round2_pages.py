"""Operate only this repository's existing Pages workflow using git's credential helper.

Credentials are held in memory and sent only to api.github.com; never printed or saved.
"""
import argparse
import json
import os
import subprocess
import urllib.error
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument("action", choices=["status", "dispatch", "pages"])
args = parser.parse_args()
environment = dict(os.environ, GIT_TERMINAL_PROMPT="0", GCM_INTERACTIVE="never")
result = subprocess.run(["git", "credential", "fill"], input="protocol=https\nhost=github.com\n\n",
                        text=True, capture_output=True, env=environment, check=True)
credential = dict(line.split("=", 1) for line in result.stdout.splitlines() if "=" in line)
base = "https://api.github.com/repos/minhtri22/boxer"
data = None
if args.action == "dispatch":
    url = base + "/actions/workflows/p0-web-deploy.yml/dispatches"
    data = json.dumps({"ref": "p1/whole-body-mechanics"}).encode()
elif args.action == "pages":
    url = base + "/pages"
else:
    url = base + "/actions/workflows/p0-web-deploy.yml/runs?branch=p1%2Fwhole-body-mechanics&per_page=3"
request = urllib.request.Request(url, data=data, headers={"Authorization": "Bearer " + credential["password"],
    "Accept": "application/vnd.github+json", "User-Agent": "boxer-round2-uat", "Content-Type": "application/json"})
try:
    with urllib.request.urlopen(request, timeout=30) as response:
        payload = response.read()
        content = json.loads(payload) if payload else {}
        if args.action == "status":
            content = [{k: run.get(k) for k in ("id", "head_sha", "status", "conclusion", "html_url", "created_at")}
                       for run in content.get("workflow_runs", [])]
        elif args.action == "pages":
            content = {k: content.get(k) for k in ("html_url", "status", "build_type")}
        print(json.dumps({"http_status": response.status, "result": content}, indent=2))
except urllib.error.HTTPError as error:
    print(json.dumps({"http_status": error.code, "message": json.loads(error.read()).get("message", "Request failed")}))
    raise SystemExit(1)
