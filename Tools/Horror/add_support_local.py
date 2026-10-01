from pathlib import Path
from blender_route import verify,execute
source=Path('Tools/Horror/refine_assets.py').read_text(encoding='utf-8')
code=source[:source.index("if bpy.data.objects.get('HORROR_refinement_marker')")]+'''\nr=bpy.data.objects.get('site_support')
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
'''
Path('Tools/Horror/author_support.py').write_text(code,encoding='utf-8')
verify();print(execute(code))
