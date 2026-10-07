"""Scoped deployment operations. Credential is memory-only, sent only to api.github.com."""
import argparse
import json
import os
import subprocess
import urllib.error
import urllib.request

parser = argparse.ArgumentParser()
parser.add_argument("action", choices=["status", "dispatch", "pages", "run", "diagnose", "allow-branch"])
parser.add_argument("--run-id", type=int)
args = parser.parse_args()
environment = dict(os.environ, GIT_TERMINAL_PROMPT="0", GCM_INTERACTIVE="never")
filled = subprocess.run(["git", "credential", "fill"],
                        input="protocol=https\nhost=github.com\n\n", text=True,
                        capture_output=True, env=environment)
if filled.returncode:
    raise SystemExit("Cached GitHub credential unavailable; no credential printed.")
credential = dict(line.split("=", 1) for line in filled.stdout.splitlines() if "=" in line)
base = "https://api.github.com/repos/minhtri22/boxer"
ref = "integration/ramirez-unity-round2"
expected_head = "e3c1208ccaaeea0b19e0995e3e4f86173f50e789"

def request(route, data=None):
    req = urllib.request.Request(base + route, data=json.dumps(data).encode() if data is not None else None,
        headers={"Authorization": "Bearer " + credential["password"], "Accept": "application/vnd.github+json",
                 "User-Agent": "ramirez-round2-authorized-pages", "Content-Type": "application/json"})
    try:
        with urllib.request.urlopen(req, timeout=30) as response:
            payload = response.read()
            return response.status, json.loads(payload) if payload else {}
    except urllib.error.HTTPError as error:
        payload = json.loads(error.read())
        print(json.dumps({"http_status": error.code, "message": payload.get("message", "Request failed")}))
        raise SystemExit(1)

def compact(run):
    return {k: run.get(k) for k in ("id", "head_branch", "head_sha", "event", "status", "conclusion", "html_url", "created_at")}

if args.action == "allow-branch":
    # Explicitly authorized by the owner: add only this exact branch; never replace rules.
    route = "/environments/github-pages"
    _, before = request(route)
    policy = before.get("deployment_branch_policy")
    if policy != {"protected_branches": False, "custom_branch_policies": True}:
        raise SystemExit("Unexpected environment policy: refusing to change protections")
    _, old = request(route + "/deployment-branch-policies?per_page=100")
    old_rules = old.get("branch_policies", [])
    if old.get("total_count") != len(old_rules):
        raise SystemExit("Incomplete policy inventory: refusing mutation")
    added = None
    if not any(p["name"] == ref and p.get("type") == "branch" for p in old_rules):
        _, added = request(route + "/deployment-branch-policies", {"name": ref, "type": "branch"})
    _, after = request(route)
    _, new = request(route + "/deployment-branch-policies?per_page=100")
    new_rules = new.get("branch_policies", [])
    protections_unchanged = all(before.get(k) == after.get(k) for k in
        ("protection_rules", "deployment_branch_policy", "wait_timer", "reviewers", "can_admins_bypass"))
    existing_preserved = all(p in new_rules for p in old_rules)
    exact_allowed = any(p["name"] == ref and p.get("type") == "branch" for p in new_rules)
    print(json.dumps({"added": added, "protections_unchanged": protections_unchanged,
        "existing_policies_preserved": existing_preserved, "exact_branch_allowed": exact_allowed,
        "branch_policies": new_rules}, indent=2))
    if not (protections_unchanged and existing_preserved and exact_allowed):
        raise SystemExit("Post-mutation verification failed; do not dispatch")
elif args.action == "diagnose":
    _, env = request("/environments/github-pages")
    print(json.dumps({"environment": {k: env.get(k) for k in ("name", "protection_rules", "deployment_branch_policy")}}, indent=2))
    _, policies = request("/environments/github-pages/deployment-branch-policies")
    print(json.dumps({"branch_policies": policies}, indent=2))
    if args.run_id:
        _, run = request("/actions/runs/" + str(args.run_id))
        _, checks = request("/check-suites/" + str(run["check_suite_id"]) + "/check-runs")
        for check in checks.get("check_runs", []):
            print(json.dumps({"check": {k: check.get(k) for k in ("id", "name", "status", "conclusion", "output")}}, indent=2))
            _, annotations = request("/check-runs/" + str(check["id"]) + "/annotations")
            print(json.dumps({"annotations": annotations}, indent=2))
elif args.action == "pages":
    status, page = request("/pages")
    print(json.dumps({"http_status": status, "result": {k: page.get(k) for k in ("html_url", "status", "build_type")}}, indent=2))
elif args.action == "run":
    if not args.run_id:
        raise SystemExit("--run-id required")
    status, run = request("/actions/runs/" + str(args.run_id))
    print(json.dumps({"http_status": status, "result": compact(run)}, indent=2))
    _, jobs = request("/actions/runs/" + str(args.run_id) + "/jobs")
    print(json.dumps({"jobs": [{"id": j["id"], "name": j["name"], "status": j["status"], "conclusion": j["conclusion"],
        "steps": [{k: s.get(k) for k in ("name", "status", "conclusion")} for s in j.get("steps", [])]}
        for j in jobs.get("jobs", [])]}, indent=2))
elif args.action == "status":
    status, runs = request("/actions/workflows/p0-web-deploy.yml/runs?per_page=5")
    print(json.dumps({"http_status": status, "result": [compact(r) for r in runs.get("workflow_runs", [])]}, indent=2))
elif args.action == "dispatch":
    _, remote = request("/git/ref/heads/" + ref)
    if remote["object"]["sha"] != expected_head:
        raise SystemExit("Remote branch changed: refusing stale candidate dispatch")
    _, runs = request("/actions/workflows/p0-web-deploy.yml/runs?per_page=10")
    if any(r["status"] != "completed" for r in runs.get("workflow_runs", [])):
        raise SystemExit("An active Pages run exists: refusing implicit cancellation")
    status, result = request("/actions/workflows/p0-web-deploy.yml/dispatches", {"ref": ref})
    print(json.dumps({"http_status": status, "ref": ref, "head_sha": expected_head, "result": result}, indent=2))
