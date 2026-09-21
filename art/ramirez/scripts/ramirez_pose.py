"""Blender-only anatomy/deformation poses; not gameplay contact or punch timing."""
import math
import bpy
from mathutils import Vector, Matrix


def set_bone(rig, name, start, end):
    p=rig.pose.bones[name];rest=rig.data.bones[name].matrix_local.to_quaternion()
    direction=(Vector(end)-Vector(start)).normalized()
    rotation=(rest @ Vector((0,1,0))).rotation_difference(direction) @ rest
    p.matrix=Matrix.Translation(start) @ rotation.to_matrix().to_4x4()
    bpy.context.view_layer.update()


def limb(rig, a, b, endpoint, pole):
    start=rig.pose.bones[a].head.copy();l1=rig.data.bones[a].length;l2=rig.data.bones[b].length
    delta=Vector(endpoint)-start;distance=min(delta.length,l1+l2-.0001)
    direction=delta.normalized();along=(l1*l1-l2*l2+distance*distance)/(2*distance)
    side=Vector(pole)-start;side=(side-direction*side.dot(direction)).normalized()
    middle=start+direction*along+side*math.sqrt(max(0,l1*l1-along*along))
    endpoint=start+direction*distance
    set_bone(rig,a,start,middle);set_bone(rig,b,middle,endpoint)
    return endpoint


def hand(rig, side, wrist, direction):
    y=Vector(direction).normalized();front=Vector((0,-1,0));z=(front-y*front.dot(y)).normalized();x=y.cross(z).normalized()
    rotation=Matrix((x,y,z)).transposed().to_4x4()
    rig.pose.bones['hand.'+side].matrix=Matrix.Translation(wrist) @ rotation
    bpy.context.view_layer.update()


def apply_pose(rig, name='guard'):
    for p in rig.pose.bones:p.matrix_basis=Matrix.Identity(4)
    shift=Vector((0,-.025,-.085));yaw=-10
    if name=='cross':yaw=18;shift.y-=.035
    if name=='hook':yaw=-25;shift.y-=.025
    if name=='uppercut':yaw=-20;shift.z+=.015
    if name=='overhand':yaw=8;shift.y-=.035
    if name=='slip':shift.x=-.035
    if name=='roll':shift.x=.035;shift.z-=.065
    if name=='advance':shift.y-=.12
    if name=='retreat':shift.y+=.12
    rig.pose.bones['root'].matrix=Matrix.Translation(shift) @ Matrix.Rotation(math.radians(yaw),4,'Z') @ rig.data.bones['root'].matrix_local
    bpy.context.view_layer.update()
    neck=rig.pose.bones['neck01'];neck.rotation_mode='XYZ';neck.rotation_euler.x=math.radians(9)
    spine=rig.pose.bones['spine02'];spine.rotation_mode='XYZ';spine.rotation_euler.x=math.radians(6)
    if name=='slip':spine.rotation_euler.z=math.radians(-12)
    if name=='roll':spine.rotation_euler.x=math.radians(13)
    bpy.context.view_layer.update()
    for side,sign in [('L',1),('R',-1)]:
        ankle=rig.data.bones['foot.'+side].head_local.copy();ankle.x=sign*.23;ankle.y+=-.16 if side=='L' else .13
        if name=='advance' and side=='L':ankle.y-=.18;ankle.z+=.015
        if name=='retreat' and side=='R':ankle.y+=.18;ankle.z+=.015
        ankle=limb(rig,'thigh.'+side,'shin.'+side,ankle,(sign*.23,-.5,.65))
        direction=rig.data.bones['foot.'+side].tail_local-rig.data.bones['foot.'+side].head_local
        set_bone(rig,'foot.'+side,ankle,ankle+direction)
        wrist=Vector((sign*.115,-.29 if side=='L' else -.23,1.34 if side=='L' else 1.385))
        pole=(sign*.30,-.1,1.07);hand_dir=(0,-.065,.08)
        if name=='jab' and side=='L':wrist=Vector((.10,-.59,1.40));pole=(.28,-.30,1.27);hand_dir=(0,-1,.08)
        if name=='cross' and side=='R':wrist=Vector((-.035,-.60,1.41));pole=(-.30,-.3,1.30);hand_dir=(0,-1,.06)
        if name=='hook' and side=='L':wrist=Vector((-.005,-.405,1.43));pole=(.48,-.12,1.44);hand_dir=(-.5,-.1,.15)
        if name=='uppercut' and side=='L':wrist=Vector((.065,-.37,1.32));pole=(.30,-.07,1.02);hand_dir=(0,-.15,1)
        if name=='overhand' and side=='R':wrist=Vector((-.025,-.50,1.48));pole=(-.40,-.23,1.65);hand_dir=(.05,-1,-.3)
        wrist=limb(rig,'upperarm.'+side,'forearm.'+side,wrist,pole)
        hand(rig,side,wrist,hand_dir)
    bpy.context.view_layer.update()
