import * as until from "util";

import * as React from "react";

import { TreeDiagram as TreeDiagram_Tree } from "../../Ice/Tree";
import { TreeDiagram as TreeDiagram_TreeView } from "../../Ice/TreeView";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";
import { Tree } from "../IceProxy/Tree";

import { ITreeEditPropsConfig as R_NodeConfig, Node as R_Node } from "./Node";

import { getLogger } from "log4js";

const logger = getLogger("TreeEdit");

export interface ITreeEditProps {
    uuid: string;
    type: TreeDiagram_Tree.TreeType;
    CallBackHandler: () => void;
}

export interface ITreeListState {
    view: TreeDiagram_TreeView.TreeView | null;
}

export class TreeEdit extends React.Component<ITreeEditProps, ITreeListState> {
    private proxy: Proxy | null = null;

    public constructor(props: ITreeEditProps) {
        super(props);
        this.state = {
            view: null,
        };
        this.init();
    }

    public render() {
        const view = this.state.view;
        logger.info(until.inspect(view));
        const nodeConfig: R_NodeConfig = {
            rootUUID: this.props.uuid,
            parentUUID: this.props.uuid,
            uuid: this.props.uuid,
            data: "",
            view,
            isRoot: true,
            isbinaryTree: this.props.type === TreeDiagram_Tree.TreeType.Binary,
        };
        return <>
            <h1 >UUID = {this.props.uuid} {this.props.type.toString()}</h1>
            <h1 >type = {this.props.type.toString()}</h1>
            <button type="button" className="btn btn-success" onClick={this.props.CallBackHandler} >回圖清單</button>
            <hr />
            <R_Node config={nodeConfig} />
            <hr />
            <button type="button" className="btn btn-success" onClick={this.props.CallBackHandler} >回圖清單</button>
        </>;
    }
    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
            this.proxy.event.event.removeListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        }
    }

    private eventTreeUpdateListener = async () => {
        return this.treeViewUpdate();
    }

    private eventTreeListUpdateListener = async () => {
        if (this.proxy !== null) {
            const trees = await this.proxy.server_listAllTrees();
            if (trees.findIndex((tree) => tree.uuid === this.props.uuid) === -1) {
                alert("本圖已被刪除");
                this.props.CallBackHandler();
            }
        }
    }

    private async treeViewUpdate() {
        if (this.proxy !== null) {
            const view = await this.proxy.server_getNodeView(this.props.uuid);
            this.setState({ view });
        }
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventTreeUpdate, this.eventTreeUpdateListener);
        this.proxy.event.event.addListener(ClientEvent.eventTreeListUpdate, this.eventTreeListUpdateListener);
        return this.treeViewUpdate();
    }
}
