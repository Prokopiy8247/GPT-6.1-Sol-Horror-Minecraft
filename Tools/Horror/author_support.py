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



r=bpy.data.objects.get('site_support')
if r is None:
    col=bpy.data.collections.new('site_support');top.children.link(col);current[0]=col
    r=bpy.data.objects.new('site_support',None);col.objects.link(r)
    loft('support_timber',[(0,0,0,.15,.15),(0,0,1,.15,.15)],'linen',r,8)
    ring('support_foot',.05,.19,.08,'copper',r,12)
    bpy.ops.object.select_all(action='DESELECT');r.select_set(True)
    for o in r.children_recursive:o.select_set(True)
    bpy.context.view_layer.objects.active=r
    bpy.ops.export_scene.fbx(filepath=OUT+'site_support.fbx',use_selection=True,axis_forward='-Z',axis_up='Y',object_types={'MESH','EMPTY'},bake_anim=False,add_leaf_bones=False)
    r.location=(40,52,0)
    bpy.ops.wm.save_as_mainfile(filepath=MASTER)
print('SITE_SUPPORT_READY')
