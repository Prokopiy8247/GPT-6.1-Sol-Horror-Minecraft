import bpy
from mathutils import Vector

MASTER='W:/YouTube/Claude Opus 5.5/Unity/Unity Minecraft/Unity Opus 5.5 Minecraft/UnityMinecraft.blend'
if bpy.data.filepath.replace('\\','/').lower()!=MASTER.lower():raise ValueError('Wrong master')
scene=bpy.data.scenes.get('HORROR_PREVIEW')
if scene is None:
    scene=bpy.data.scenes.new('HORROR_PREVIEW')
    scene.collection.children.link(bpy.data.collections['unseam'])
    col=bpy.data.collections.new('HORROR_PreviewSetup')
    bpy.data.collections['HORROR_EXPANSION'].children.link(col)
    scene.collection.children.link(col)
    data=bpy.data.cameras.new('HORROR_Camera')
    camera=bpy.data.objects.new('HORROR_Camera',data);col.objects.link(camera)
    camera.location=(7,9,5.0)
    camera.rotation_euler=(Vector((0,20,2.7))-camera.location).to_track_quat('-Z','Y').to_euler()
    data.lens=55
    scene.camera=camera
    for name,pos,color,power,size in [('key',(3,15,7),(0.73,0.84,1),1700,5),('rim',(-4,24,5),(0.20,0.75,0.72),2100,4),('fill',(-5,12,3),(0.74,0.55,0.35),700,5)]:
        d=bpy.data.lights.new('HORROR_'+name,'AREA');d.energy=power;d.color=color;d.shape='DISK';d.size=size
        o=bpy.data.objects.new('HORROR_'+name,d);col.objects.link(o);o.location=pos;o.rotation_euler=(Vector((0,20,2.5))-o.location).to_track_quat('-Z','Y').to_euler()
    world=bpy.data.worlds.new('HORROR_PreviewWorld');world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(0.055,0.07,0.085,1);scene.world=world
scene.render.engine='CYCLES'
scene.cycles.samples=24
scene.render.resolution_x=1100;scene.render.resolution_y=1100;scene.render.resolution_percentage=100
scene.render.filepath='W:/YouTube/Claude_Opus_Horror_Blender.png'
bpy.ops.render.render(write_still=True,scene=scene.name)
bpy.ops.wm.save_as_mainfile(filepath=MASTER)
print('HORROR_PREVIEW_SAVED')
