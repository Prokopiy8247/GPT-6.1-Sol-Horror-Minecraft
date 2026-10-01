import bpy
import math
MASTER='W:/YouTube/Claude Opus 5.5/Unity/Unity Minecraft/Unity Opus 5.5 Minecraft/UnityMinecraft.blend'
OUT='W:/YouTube/Claude Opus 5.5/Unity/Unity Minecraft/Unity Opus 5.5 Minecraft/Assets/_Game/Horror/Resources/Horror/Models/'
if bpy.data.filepath.replace('\\','/').lower()!=MASTER.lower():raise ValueError('Wrong master')
top=bpy.data.collections['HORROR_EXPANSION']
palette={n:bpy.data.materials['HORROR_'+n] for n in ['ash','ivory','copper','dark','blue','red','linen']}
current=[None]
original=[(o.name,len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.data.objects if not any(c==top or c in top.children_recursive for c in o.users_collection)]
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


if bpy.data.objects.get('HORROR_refinement_marker'):raise ValueError('Refinement already applied')
marker=bpy.data.objects.new('HORROR_refinement_marker',None);top.objects.link(marker)
r=bpy.data.objects['unseam'];current[0]=bpy.data.collections['unseam']
body=next(o for o in r.children_recursive if o.name=='body')
head=next(o for o in r.children_recursive if o.name=='head')
for i in range(12):
    a=i*math.tau/12
    loft('mantle_strand_'+str(i),[(math.cos(a)*.72,math.sin(a)*.51,.7,.055,.04),(math.cos(a)*.55,math.sin(a)*.41,-.1,.045,.025),(math.cos(a)*.37,math.sin(a)*.30,-1.15-(i%3)*.15,.01,.008)],'ash',body,6)
for i in range(6):
    loft('split_crown_'+str(i),[((i-2.5)*.10,.08,.35,.045,.035),((i-2.5)*.12,.10,1.20+(i%2)*.08,.025,.02),((i-2.5)*.09,.12,1.4,.004,.004)],'ivory',head,6)
for i in range(7):
    slab('mantle_stitch_'+str(i),(.16,-.58,.15+i*.15),(.19,.014,.027),'copper',body)
changed=[r]
for name in ['survey_cairn','listening_well','shutter_chapel','root_archive','open_bell']:
    r=bpy.data.objects[name];current[0]=bpy.data.collections[name]
    for obj in list(r.children_recursive):bpy.data.objects.remove(obj,do_unlink=True)
    if name=='survey_cairn':
        for i in range(5):
            block=slab('cairn_layer_'+str(i),(.08*(i%2),0,.13+i*.25),(1.55-i*.21,1.1-i*.14,.24),'ash',r);block.rotation_euler.z=i*.19
        slab('survey_tablet',(0,-.50,.72),(.54,.06,.72),'ivory',r)
        for i in range(4):slab('tablet_glyph_'+str(i),(.10*(-1 if i%2 else 1),-.54,.48+i*.13),(.18,.02,.035),'blue',r)
    elif name=='listening_well':
        for i in range(4):ring('well_course_'+str(i),.12+i*.22,1.10,.19,'ash',r,16)
        for side in (-1,1):slab('well_upright_'+str(side),(side*.95,0,1.5),(.20,.22,2.1),'linen',r)
        slab('well_beam',(0,0,2.4),(2.6,.26,.22),'copper',r)
        loft('hanging_listener',[(0,0,1.1,.16,.16),(0,0,1.7,.24,.24),(0,0,1.95,.035,.035)],'ivory',r)
        slab('well_chain',(0,0,2.1),(.04,.04,.5),'copper',r)
    elif name=='shutter_chapel':
        slab('chapel_floor',(0,0,.08),(2.7,2.7,.16),'ash',r)
        for side in (-1,1):
            slab('chapel_wall_'+str(side),(side*1.2,.45,1.15),(.20,1.8,2.3),'dark',r)
            for i in range(5):slab('chapel_shutter_'+str(side)+'_'+str(i),(side*.63,1.23,.33+i*.38),(1.08,.12,.22),'ivory',r)
            beam=slab('chapel_roof_'+str(side),(side*.67,0,2.65),(1.5,2.65,.16),'linen',r);beam.rotation_euler.y=side*.40
        slab('chapel_altar',(0,.55,.6),(.9,.65,1.0),'copper',r)
    elif name=='root_archive':
        for side in (-1,1):
            slab('archive_shelf_'+str(side),(side*.92,.6,1.15),(.30,1.5,2.3),'linen',r)
            for i in range(6):
                slab('archive_record_'+str(side)+'_'+str(i),(side*.94,.38,.3+i*.32),(.40,.84,.16),'ivory',r)
        for i in range(8):
            a=i*math.tau/8
            loft('archive_root_'+str(i),[(math.cos(a)*1.3,math.sin(a)*1.3,.1,.12,.12),(math.cos(a)*.9,math.sin(a)*.9,1.4,.10,.09),(math.cos(a)*.5,math.sin(a)*.5,2.75,.035,.03)],'ash',r,6)
        slab('archive_desk',(0,-.55,.58),(1.2,.65,.14),'copper',r)
        ring('archive_thread',1.8,.42,.035,'blue',r)
    else:
        ring('bell_dais',.08,2.4,.22,'ash',r,32)
        for side in (-1,1):slab('bell_gate_'+str(side),(side*1.72,0,1.8),(.40,.52,3.6),'ivory',r)
        slab('bell_lintel',(0,0,3.6),(3.9,.55,.4),'ash',r)
        loft('open_bell_shell',[(0,0,2.1,.80,.65),(0,0,2.35,.61,.5),(0,0,2.9,.30,.26),(0,0,3.3,.12,.10)],'copper',r,16)
        for side in (-1,1):slab('bell_seam_'+str(side),(side*.30,-.6,2.48),(.12,.1,.76),'ivory',r)
    loft('source_splinter',[(0,0,.20,.16,.13),(0,0,.8,.08,.07),(0,0,1.08,.008,.008)],'blue',r,8)
    changed.append(r)
for name in ['clatter_lure','resonant_splinter','quiet_heart','shutter_lens','bell_key']:
    r=bpy.data.objects[name];current[0]=bpy.data.collections[name]
    if name in ['clatter_lure','resonant_splinter','quiet_heart']:
        for obj in list(r.children_recursive):bpy.data.objects.remove(obj,do_unlink=True)
    if name=='clatter_lure':
        for i in range(5):
            a=i*math.tau/5
            loft('clatter_pipe_'+str(i),[(math.cos(a)*.14,math.sin(a)*.14,.05,.035,.035),(math.cos(a)*.14,math.sin(a)*.14,.45+(i%2)*.12,.045,.045)],'copper',r,8)
        ring('clatter_cage',.28,.23,.028,'ivory',r)
    elif name=='resonant_splinter':
        for i in range(3):loft('splinter_'+str(i),[((i-1)*.10,0,.03,.06,.04),((i-1)*.07,0,.22,.09,.04),((i-1)*.10,0,.6-i*.08,.005,.005)],'blue',r,6)
    elif name=='quiet_heart':
        loft('quiet_relic',[(0,0,.03,.12,.10),(0,0,.23,.23,.14),(0,0,.4,.13,.12),(0,0,.57,.02,.02)],'dark',r,10)
        for i in range(3):ring('relic_circuit_'+str(i),.16+i*.10,.16,.025,'blue',r)
    elif name=='shutter_lens':slab('lens_grip',(0,0,-.2),(.08,.08,.4),'dark',r)
    else:
        for i in range(3):slab('key_tooth_'+str(i),(.10,0,.2+i*.1),(.20,.09,.035),'blue',r)
    changed.append(r)
for root in changed:
    location=root.location.copy();root.location=(0,0,0)
    bpy.ops.object.select_all(action='DESELECT');root.select_set(True)
    for obj in root.children_recursive:obj.select_set(True)
    bpy.context.view_layer.objects.active=root
    bpy.ops.export_scene.fbx(filepath=OUT+root.name+'.fbx',use_selection=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,add_leaf_bones=False)
    root.location=location
    print('REFINED',root.name,len(root.children_recursive))
after=[(o.name,len(o.data.vertices) if o.type=='MESH' else 0) for o in bpy.data.objects]
print('ORIGINAL_PRESERVED',all(entry in after for entry in original),len(original))
bpy.ops.wm.save_as_mainfile(filepath=MASTER)
