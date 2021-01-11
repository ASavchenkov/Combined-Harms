using Godot;
using System;
using System.Collections.Generic;

public class RifleFPV : Spatial, IObserver
{
    
    [Export]
    Dictionary<string,NodePath> attachmentMap;

    RifleProvider provider;

    protected Spatial Muzzle;
    protected IMunitionSource source;
    protected Spatial Origin;
    protected SightFPVObserver MainSight;
    //When we start to do things like canted sights
    //we will need to revisit this.

    [Export]
    public float muzzleVelocity = 10;//In meters per second I think?

    //camera recoil effects can really only happen in the x and y directions,
    //so we don't worry about using full transforms.
    // (Though we should experiment with z recoil.)
    [Signal]
    public delegate void Recoil(float x, float y);

    private Node Projectiles;

    public override void _Ready()
    {
        Projectiles = GetNode("/root/GameRoot/Projectiles");
    }

    public void Subscribe(Node provider)
    {
        this.provider = (RifleProvider) provider;
        this.provider.Connect(nameof(RifleProvider.AttachmentUpdated), this, nameof(OnAttachmentUpdated));
    }

    //Default code for firing a projectile.
    //Unlikely anyone will need to modify this very much.
    public virtual void Fire()
    {
        
        string projectileScene = source.DequeueMunition();
        if(!(projectileScene is null))
        {
            Vector3 velocity = Muzzle.GlobalTransform.basis.Xform(-Vector3.Back) * muzzleVelocity;
            
            ProjectileProvider p = EasyInstancer.Instance<ProjectileProvider>(projectileScene);
            p.Rpc("Init",Muzzle.GlobalTransform.origin, velocity);
        }

        
    }

    public void OnAttachmentUpdated(string attachPoint, IFPV attachment)
    {
        Node parentNode = GetNode(attachmentMap[attachPoint]);
        for(int i = 0; i<parentNode.GetChildCount(); i++)
        {
            parentNode.GetChild(i).QueueFree();
        }
        //If null was passed, there is no attachment.
        if(!(attachment is null))
        {
            var observer = EasyInstancer.Instance<Node>(attachment.ObserverPathFPV);
            parentNode.AddChild(EasyInstancer.GenObserver((Node) attachment, attachment.ObserverPathFPV));
        }
        
    }
}
