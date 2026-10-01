import bpy
import math
from mathutils import Vector

# Authored profiles, seams, hollow masks and pivot hierarchies. Only this collection is modified.
MASTER = 'W:/YouTube/Claude Opus 5.5/Unity/Unity Minecraft/Unity Opus 5.5 Minecraft/UnityMinecraft.blend'
OUT = 'W:/YouTube/Claude Opus 5.5/Unity/Unity Minecraft/Unity Opus 5.5 Minecraft/Assets/_Game/Horror/Resources/Horror/Models/'
if bpy.data.filepath.replace('\\', '/').lower() != MASTER.lower():
    raise ValueError('Wrong project master')
if bpy.data.collections.get('HORROR_EXPANSION'):
    raise ValueError('Collection already exists: update individual assets rather than duplicate them')
original = [(o.name, o.type, len(o.data.vertices) if o.type == 'MESH' else 0) for o in bpy.data.objects]
old_selected = list(bpy.context.selected_objects)
old_active = bpy.context.view_layer.objects.active
top = bpy.data.collections.new('HORROR_EXPANSION')
bpy.context.scene.collection.children.link(top)
palette = {}
for name, color in [('ash',(0.14,0.19,0.20,1)),('ivory',(0.72,0.69,0.52,1)),('copper',(0.38,0.19,0.09,1)),('dark',(0.035,0.043,0.052,1)),('blue',(0.10,0.70,0.72,1)),('red',(0.61,0.20,0.12,1)),('linen',(0.38,0.42,0.32,1))]:
    m=bpy.data.materials.new('HORROR_'+name)
    m.diffuse_color=color
    m.use_nodes=True
    node=m.node_tree.nodes.get('Principled BSDF')
    node.inputs['Base Color'].default_value=color
    node.inputs['Roughness'].default_value=0.88
    palette[name]=m

current=[None]
roots=[]
def asset(name):
    current[0]=bpy.data.collections.new(name)
    top.children.link(current[0])
    root=bpy.data.objects.new(name,None)
    current[0].objects.link(root)
    roots.append(root)
    return root

def pivot(name,pos,parent):
    o=bpy.data.objects.new(name,None)
    current[0].objects.link(o)
    o.parent=parent
    o.location=pos
    return o

def mesh(name,verts,faces,mat,parent,pos=(0,0,0)):
    d=bpy.data.meshes.new('HORROR_'+name)
    d.from_pydata(verts,[],faces)
    d.update()
    o=bpy.data.objects.new(name,d)
    current[0].objects.link(o)
    o.parent=parent
    o.location=pos
    d.materials.append(palette[mat])
    uv=d.uv_layers.new(name='UVMap')
    for poly in d.polygons:
        for li in poly.loop_indices:
            v=d.vertices[d.loops[li].vertex_index].co
            uv.data[li].uv=(v.x*0.37+v.y*0.13,v.z*0.37)
    return o

def loft(name,profile,mat,parent,n=10):
    vs=[]
    for x,y,z,rx,ry in profile:
        for i in range(n):
            a=i*math.tau/n
            # alternately recessed ribs produce a deliberate faceted skin
            f=1 if i%2==0 else 0.92
            vs.append((x+math.cos(a)*rx*f,y+math.sin(a)*ry*f,z))
    fs=[]
    for r in range(len(profile)-1):
        for i in range(n):
            a=r*n+i;b=r*n+(i+1)%n
            fs.append((a,b,b+n,a+n))
    fs.append(tuple(range(n-1,-1,-1)))
    fs.append(tuple((len(profile)-1)*n+i for i in range(n)))
    return mesh(name,vs,fs,mat,parent)

def slab(name,pos,size,mat,parent):
    x,y,z=size
    vs=[(-x/2,-y/2,-z/2),(x/2,-y/2,-z/2),(x/2,y/2,-z/2),(-x/2,y/2,-z/2),(-x/2,-y/2,z/2),(x/2,-y/2,z/2),(x/2,y/2,z/2),(-x/2,y/2,z/2)]
    return mesh(name,vs,[(0,3,2,1),(4,5,6,7),(0,1,5,4),(1,2,6,5),(2,3,7,6),(3,0,4,7)],mat,parent,pos)

def ring(name,z,r,thick,mat,parent,n=24):
    vs=[]
    for zz in (z-thick/2,z+thick/2):
        for rr in (r,r-thick):
            for i in range(n):
                a=i*math.tau/n;vs.append((rr*math.cos(a),rr*math.sin(a),zz))
    fs=[]
    for i in range(n):
        j=(i+1)%n
        fs.extend([(i,j,n+j,n+i),(2*n+i,3*n+i,3*n+j,2*n+j),(i,2*n+i,2*n+j,j),(n+i,n+j,3*n+j,3*n+i)])
    return mesh(name,vs,fs,mat,parent)

def limb(name,pos,length,lean,parent,mat='ash',radius=0.16):
    p=pivot(name,pos,parent)
    loft(name+'_skin',[(0,0,0,radius,radius*0.7),(lean*0.25,0,-length*0.35,radius*0.8,radius*0.55),(lean,0,-length*0.8,radius*0.55,radius*0.4),(lean*0.9,-0.13,-length,radius*0.34,radius*0.24)],mat,p)
    ring(name+'_joint',-0.08,radius*1.12,0.035,'ivory',p,12)
    foot=pivot(name+'_foot',(lean*0.9,-0.13,-length),p)
    loft(name+'_hook',[(0,0,0,radius*0.4,radius*0.35),(0,-0.22,0.04,radius*0.5,radius*0.3),(0,-0.46,-0.01,0.015,0.025)],'dark',foot)
    return p

r=asset('unseam')
body=pivot('body',(0,0,2.8),r)
loft('oblique_mantle',[(-0.22,0,0,0.75,0.53),(0.18,0.06,0.55,0.94,0.65),(0.30,0.04,1.0,0.65,0.44),(0.22,-0.02,1.55,0.28,0.22)],'dark',body,16)
for i in range(9):
    z=0.12+i*0.14
    for side in (-1,1):
        loft('mantle_rib_'+str(i)+'_'+str(side),[(side*0.06,-0.55,z,0.07,0.045),(side*(0.7-i*0.025),-0.31,z+0.2,0.075,0.045),(side*(0.88-i*0.03),0.07,z+0.32,0.035,0.025)],'ash',body,8)
head=pivot('head',(0.2,-0.10,1.22),body)
# Six disconnected ivory shutters surround a deep, narrow aperture. No skull/eyes/teeth.
for side in (-1,1):
    shutter=pivot('shutter_'+str(side),(side*0.08,-0.24,0.55),head)
    for i in range(4):
        loft('face_shutter_'+str(side)+'_'+str(i),[(side*0.20,-0.08,-0.48+i*0.24,0.19,0.055),(side*0.30,-0.22,-0.34+i*0.24,0.16,0.06),(side*0.20,-0.30,-0.13+i*0.24,0.07,0.035)],'ivory',shutter,8)
slab('aperture',(0,-0.15,0.68),(0.11,0.06,1.12),'blue',head)
loft('head_keel',[(0,0,-0.12,0.15,0.2),(0,0.06,0.8,0.13,0.18),(0.12,0,1.45,0.035,0.03)],'ash',head)
for i,(x,y,l,lean) in enumerate([(-0.7,-0.15,2.7,-0.36),(0.65,-0.25,2.65,0.45),(0.04,0.60,2.75,-0.10)]):
    limb('leg_'+str(i),(x,y,2.8),l,lean,r,radius=0.19)
for i,side in enumerate((-1,1)):
    arm=limb('arm_'+str(i),(side*0.84,-0.07,0.8),2.1,side*0.42,body,radius=0.17)
    for k in range(3):
        loft('arm_tine_'+str(i)+'_'+str(k),[(side*0.42,-0.18,-1.9,0.06,0.045),(side*(0.5+k*0.08),-0.42,-2.08-k*0.1,0.05,0.035),(side*0.35,-0.68,-2.1-k*0.1,0.012,0.012)],'ivory',arm,6)

r=asset('rattleblind')
b=pivot('body',(0,0,0.9),r)
loft('resonant_back',[(0,0,-0.1,0.46,0.65),(0,0,0.3,0.52,0.62),(0,-0.15,0.7,0.25,0.3)],'ash',b)
h=pivot('head',(0,-0.4,0.4),b)
for i in range(5):ring('hearing_plate_'+str(i),i*0.1,0.33-i*0.025,0.04,'ivory',h)
for i in range(4):limb('leg_'+str(i),((-1 if i%2 else 1)*0.38,(-0.40 if i<2 else 0.38),0.9),0.88,(-1 if i%2 else 1)*0.23,r,radius=0.10)

r=asset('lintel')
b=pivot('body',(0,0,1.7),r)
loft('folded_spindle',[(0,0,-0.7,0.25,0.16),(0,0,0,0.48,0.24),(0,0,0.8,0.13,0.10)],'linen',b)
h=pivot('head',(0,-0.12,0.2),b)
ring('blind_lintel',0.45,0.22,0.055,'ivory',h)
for i in range(4):limb('arm_'+str(i),((-1 if i%2 else 1)*0.35,0,0.35-i*0.2),1.3,(-1 if i%2 else 1)*0.45,b,radius=0.07)

r=asset('hearthmimic')
b=pivot('body',(0,0,0.7),r)
loft('bellied_reliquary',[(0,0,-0.65,0.5,0.30),(0,0,0.05,0.48,0.33),(0,0,0.45,0.3,0.25)],'copper',b)
h=pivot('head',(0,-0.1,0.3),b)
for i in range(6):
    a=i*math.tau/6
    slab('false_latch_'+str(i),(math.cos(a)*0.2,math.sin(a)*0.2,0.15),(0.065,0.065,0.36),'ivory',h)
for i in range(2):limb('leg_'+str(i),((-1 if i==0 else 1)*0.37,0,0.8),0.78,(-1 if i==0 else 1)*0.32,r,'dark',0.11)

r=asset('wickdrinker')
b=pivot('body',(0,0,1.1),r)
loft('quenched_cowl',[(0,0,-1.05,0.36,0.24),(0,0,-0.4,0.24,0.23),(0,0,0.55,0.48,0.32),(0,0,1.15,0.22,0.16)],'dark',b,16)
h=pivot('head',(0,-0.15,0.62),b)
for side in (-1,1):loft('wick_jaw_'+str(side),[(side*0.12,0,0.15,0.13,0.09),(side*0.20,-0.25,0.30,0.075,0.065),(side*0.07,-0.45,0.5,0.03,0.02)],'ivory',h)
for i in range(2):limb('arm_'+str(i),((-1 if i==0 else 1)*0.43,0,0.6),1.4,(-1 if i==0 else 1)*0.1,b,radius=0.08)

r=asset('stillwright')
b=pivot('body',(0,0,1.3),r)
loft('leaning_scribe',[(0,0,-0.2,0.20,0.17),(0,0,0.55,0.25,0.21),(0,0,1.0,0.16,0.14)],'ash',b)
h=pivot('head',(0,0,0.9),b)
for i in range(3):
    s=slab('crossed_veil_'+str(i),(0,-0.10,0.12+i*0.24),(0.62-i*0.05,0.06,0.19),'ivory',h)
    s.rotation_euler.y=0.2*(-1 if i%2 else 1)
for i in range(2):
    side=-1 if i==0 else 1
    limb('leg_'+str(i),(side*0.14,0,1.3),1.28,side*0.1,r,radius=0.08)
    limb('arm_'+str(i),(side*0.27,0,0.65),1.5,side*0.15,b,radius=0.065)

r=asset('briarchoir')
b=pivot('body',(0,0,0.6),r)
loft('root_column',[(0,0,-0.5,0.48,0.40),(0,0,0.3,0.3,0.28),(0,0,1.4,0.15,0.12)],'linen',b)
h=pivot('head',(0,0,1.4),b)
ring('choir_crown',0.12,0.55,0.1,'copper',h)
for i in range(6):
    a=i*math.tau/6
    loft('root_'+str(i),[(math.cos(a)*0.2,math.sin(a)*0.2,0.05,0.1,0.07),(math.cos(a)*0.6,math.sin(a)*0.6,-0.32,0.08,0.06),(math.cos(a)*0.9,math.sin(a)*0.9,-0.58,0.035,0.025)],'ash',b,6)
    loft('pipe_'+str(i),[(math.cos(a)*0.35,math.sin(a)*0.35,0,0.065,0.06),(math.cos(a)*0.45,math.sin(a)*0.45,0.60+i*0.04,0.07,0.06)],'ivory',h,8)

for name in ['ward_lantern','clatter_lure','binding_spool','tuning_fork','listening_compass','field_journal','reveal_dust','hush_balm','resonant_splinter','shutter_lens','bell_key','quiet_heart']:
    r=asset(name)
    if name=='ward_lantern':
        ring('lantern_base',0.05,0.23,0.06,'copper',r)
        ring('lantern_crown',0.6,0.18,0.05,'copper',r)
        loft('ward_core',[(0,0,0.14,0.1,0.1),(0,0,0.4,0.13,0.13),(0,0,0.55,0.05,0.05)],'blue',r)
        for i in range(4):
            a=i*math.tau/4
            loft('lantern_bar_'+str(i),[(math.cos(a)*0.16,math.sin(a)*0.16,0.1,0.025,0.025),(math.cos(a)*0.16,math.sin(a)*0.16,0.6,0.025,0.025)],'ivory',r,6)
    elif name=='tuning_fork' or name=='bell_key':
        slab('handle',(0,0,0.28),(0.08,0.08,0.5),'dark',r)
        slab('bridge',(0,0,0.5),(0.3,0.07,0.08),'copper',r)
        for side in (-1,1):slab('tine_'+str(side),(side*0.13,0,0.72),(0.065,0.065,0.42),'ivory',r)
    elif name=='field_journal':
        slab('pages',(0,0,0.3),(0.42,0.12,0.58),'ivory',r)
        for side in (-1,1):slab('cover_'+str(side),(0,side*0.075,0.3),(0.46,0.025,0.62),'linen',r)
        slab('clasp',(0.21,-0.09,0.3),(0.08,0.03,0.11),'copper',r)
    elif name=='binding_spool':
        loft('spool',[(0,0,0,0.2,0.2),(0,0,0.08,0.2,0.2),(0,0,0.09,0.12,0.12),(0,0,0.40,0.12,0.12),(0,0,0.41,0.2,0.2),(0,0,0.49,0.2,0.2)],'linen',r,16)
        for i in range(8):ring('thread_'+str(i),0.1+i*0.04,0.14,0.015,'blue',r,16)
    elif name=='listening_compass' or name=='shutter_lens':
        ring('outer_dial',0.12,0.25,0.065,'copper',r)
        ring('inner_dial',0.13,0.17,0.04,'ivory',r)
        slab('pointer',(0,0,0.16),(0.025,0.3,0.025),'blue',r)
    else:
        loft('vessel',[(0,0,0,0.1,0.09),(0,0,0.08,0.19,0.15),(0,0,0.28,0.2,0.15),(0,0,0.45,0.07,0.06),(0,0,0.52,0.04,0.04)],'ivory' if name in ['quiet_heart','resonant_splinter'] else 'copper',r,12)
        ring('seal',0.31,0.16,0.04,'blue',r,12)

for name in ['survey_cairn','listening_well','shutter_chapel','root_archive','open_bell']:
    r=asset(name)
    radius=1.1 if name!='open_bell' else 2.4
    ring('site_foundation',0.08,radius,0.22,'ash',r,32)
    for i in range(6):
        a=i*math.tau/6
        pillar=pivot('pillar_'+str(i),(math.cos(a)*radius*0.8,math.sin(a)*radius*0.8,0),r)
        loft('pillar_stone_'+str(i),[(0,0,0.1,0.12,0.12),(0,0,1.4,0.12,0.1),(0.03,0,1.6,0.08,0.07)],'ivory',pillar,8)
    ring('site_resonator',1.4,radius*0.52,0.12,'copper',r,24)
    loft('source_splinter',[(0,0,0.18,0.25,0.22),(0,0,0.7,0.15,0.13),(0,0,1.15,0.025,0.025)],'blue',r,8)

for root in roots:
    bpy.ops.object.select_all(action='DESELECT')
    root.select_set(True)
    for obj in root.children_recursive:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=OUT+root.name+'.fbx',use_selection=True,apply_unit_scale=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,add_leaf_bones=False,path_mode='AUTO')
    print('HORROR_EXPORT',root.name,len(root.children_recursive))
for i,root in enumerate(roots):root.location=(i%6*8,20+i//6*8,0)
bpy.ops.object.select_all(action='DESELECT')
for o in old_selected:o.select_set(True)
bpy.context.view_layer.objects.active=old_active
bpy.ops.wm.save_as_mainfile(filepath=MASTER)
after=[(o.name,o.type,len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.data.objects if not o.name.startswith('HORROR_') and o not in roots and not any(o in r.children_recursive for r in roots)]
print('HORROR_PRESERVED_ORIGINAL_OBJECTS',len(original),all(entry in after for entry in original))
print('HORROR_ASSETS',len(roots))
