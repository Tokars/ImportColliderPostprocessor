Colliders import post-processor hooks with action `Generate Collider` on a model import settings.

# How to use:
Create colliders low-poly meshes in your 3D editor. Name with preferred collider prefix.
- `UBX_` is a box collider
- `UCP_` is a capsule collider
- `USP_` is a sphere collider
- `UCX_` is a convex mesh collider
- `UMC_` is a mesh collider

![Example](https://github.com/Tokars/ImportColliderPostprocessor/blob/master/screenshots/blender_setup.gif)

In a unity editor menu enable/disable post-processor hook
`Assets > PostProcess Collider Generation`.
On model import settings `Generate Collider`.

![Example](https://github.com/Tokars/ImportColliderPostprocessor/blob/master/screenshots/unity_box_colliders.png)


## Install UPM:
```git
https://github.com/Tokars/ImportColliderPostprocessor.git#upm
```

package based-on tutorial [sources](https://bronsonzgeb.com/index.php/2021/11/27/better-collider-generation-with-asset-processors/) by [bronsonzgeb.com](bronsonzgeb.com).

