# Boxer UAT3 - Visual v11 clean WebGL external validation

Generated: 2026-09-17 07:10:47 +07:00

BRANCH=p1/uat3-visual-control-fix
SOURCE_SHA=f4c35080912b7625254cf97b1a42f5793cab0435
SCRATCH=D:\WORK\RESEARCH\POVGame\boxer-uat3-webgl-20260916-182723
OUTPUT=D:\WORK\RESEARCH\POVGame\boxer-uat3-webgl-20260916-182723\builds\web\boxer-round2

## Gates

BUILD_EXIT=0
BUILD_SUCCESS=True
VERIFY_EXIT=0
VERIFY_PASS=True
SOURCE_CLEAN_AFTER_BUILD=True

VERDICT=IMPLEMENTATION_PASS_PENDING_HUMAN_UAT

## Provenance

source_sha=f4c35080912b7625254cf97b1a42f5793cab0435
productVersion=ev-f4c35080912b7625254cf97b1a42f5793cab0435
unity=6000.5.8f1
result=Succeeded
build_seconds=947.5656337
total_bytes=43570198
contact=shared_anatomical_pose_relative_sphere_sweep_240Hz
presentation=P1_EV_reference_textured_articulated_3d_rig_no_bottom_controls
desktop_validation=explicit_query_flag_synthetic_no_sensor_claim
index.html=b25af5906361e005a5c025ebdb99964ee671c52d5404274ef8ab859841028dea
Build/boxer-round2.data=1867cd8d6a1ffa789b520f58c9a13823613cf8175c941bb795e58148c764b28d
Build/boxer-round2.framework.js=799a99c317831405e815dcfa114f1a3d11a0ce6cd2af13d2198693dd7555487d
Build/boxer-round2.loader.js=b4c99561081fc0e0670fa6d060f91f8eac16c715ccf9d2aaea5f8b4c24db8de1
Build/boxer-round2.wasm=2c31ff9d0d79ed814b407dcad44117645c302e4283df9a582d8722ec1bd7cf2b
TemplateData/style.css=227ec384516a47fbbf70a4d05e8e3804ea8f063009d42d8c570b72909f6c6b76

## Artifact SHA256

Build/boxer-round2.data=1867cd8d6a1ffa789b520f58c9a13823613cf8175c941bb795e58148c764b28d
Build/boxer-round2.framework.js=799a99c317831405e815dcfa114f1a3d11a0ce6cd2af13d2198693dd7555487d
Build/boxer-round2.loader.js=b4c99561081fc0e0670fa6d060f91f8eac16c715ccf9d2aaea5f8b4c24db8de1
Build/boxer-round2.wasm=2c31ff9d0d79ed814b407dcad44117645c302e4283df9a582d8722ec1bd7cf2b
index.html=b25af5906361e005a5c025ebdb99964ee671c52d5404274ef8ab859841028dea
provenance.txt=2d22587da8b3454f93bebe7c032ba13aaefde98d1de8bbb91ff7ef4393151dd2
TemplateData/style.css=227ec384516a47fbbf70a4d05e8e3804ea8f063009d42d8c570b72909f6c6b76

## Artifact verifier

{
  "result": "PASS",
  "source_sha": "f4c35080912b7625254cf97b1a42f5793cab0435",
  "productVersion": "ev-f4c35080912b7625254cf97b1a42f5793cab0435",
  "public_url": null,
  "unity": "6000.5.8f1",
  "manifest_sha256": "9362ab03cbd00552ded8f91667a015528dd771788ddb92b01bf03aba5ff524b6",
  "files": {
    "Build/boxer-round2.data": {
      "sha256": "1867cd8d6a1ffa789b520f58c9a13823613cf8175c941bb795e58148c764b28d",
      "bytes": 25707423
    },
    "Build/boxer-round2.framework.js": {
      "sha256": "799a99c317831405e815dcfa114f1a3d11a0ce6cd2af13d2198693dd7555487d",
      "bytes": 323523
    },
    "Build/boxer-round2.loader.js": {
      "sha256": "b4c99561081fc0e0670fa6d060f91f8eac16c715ccf9d2aaea5f8b4c24db8de1",
      "bytes": 27222
    },
    "Build/boxer-round2.wasm": {
      "sha256": "2c31ff9d0d79ed814b407dcad44117645c302e4283df9a582d8722ec1bd7cf2b",
      "bytes": 17503472
    },
    "index.html": {
      "sha256": "b25af5906361e005a5c025ebdb99964ee671c52d5404274ef8ab859841028dea",
      "bytes": 7400
    },
    "provenance.txt": {
      "sha256": "2d22587da8b3454f93bebe7c032ba13aaefde98d1de8bbb91ff7ef4393151dd2",
      "bytes": 940
    },
    "TemplateData/style.css": {
      "sha256": "227ec384516a47fbbf70a4d05e8e3804ea8f063009d42d8c570b72909f6c6b76",
      "bytes": 1158
    }
  }
}


## Source diff after build

CLEAN

## Local HTTP smoke

- `index.html`: HTTP 200, 7400 bytes, SHA256 `b25af5906361e005a5c025ebdb99964ee671c52d5404274ef8ab859841028dea`
- `Build/boxer-round2.data`: HTTP 200, 25707423 bytes, SHA256 `1867cd8d6a1ffa789b520f58c9a13823613cf8175c941bb795e58148c764b28d`
- `Build/boxer-round2.wasm`: HTTP 200, 17503472 bytes, SHA256 `2c31ff9d0d79ed814b407dcad44117645c302e4283df9a582d8722ec1bd7cf2b`
- Browser/WebGL interactive startup was not claimed from this local smoke because no headless browser was available in the execution environment.

Recovered from the completed external build after the original runner hit a Windows PowerShell 5.1 path-formatting incompatibility. No push, merge, deploy or Human-UAT claim was performed.
