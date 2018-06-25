import * as until from "util";

import * as React from "react";

import { getLogger } from "log4js";

import { TreeDiagram as TreeDiagram_Node } from "../../Ice/Node";
import { TreeDiagram as TreeDiagram_TreeView } from "../../Ice/TreeView";

import { ClientEvent } from "../IceProxy/ClientEvent";
import { Proxy } from "../IceProxy/IceProxy";

const logger = getLogger("Node");

export interface ITreeEditPropsConfig {
    isbinaryTree: boolean;
    rootUUID: string;
    parentUUID: string;
    isRoot: boolean;
    uuid: string;
    data: string;
    view: TreeDiagram_TreeView.TreeView | null;
    isBinaryleft: boolean;
}

export interface ITreeEditProps {
    config: ITreeEditPropsConfig;
    moveUUID: string;
    setMoveUUID: (uuid: string) => void;
}

export interface ITreeListState {
    data: string;
    childNodeData: string;
}

export class Node extends React.Component<ITreeEditProps, ITreeListState> {
    private proxy: Proxy | null = null;

    public constructor(props: ITreeEditProps) {
        super(props);
        const config = this.props.config;
        this.state = {
            data: config.data,
            childNodeData: "",
        };
        this.init();
    }

    public renderChildren(nodes: TreeDiagram_Node.Node[]) {
        const rows: JSX.Element[] = [];
        const config = this.props.config;
        let i = 0;
        for (const node of nodes) {
            const childConfig: ITreeEditPropsConfig = {
                rootUUID: config.rootUUID,
                parentUUID: config.uuid,
                isRoot: false,
                uuid: node.uuid,
                isbinaryTree: config.isbinaryTree,
                data: node.data,
                view: config.view,
                isBinaryleft: node.isBinaryleft,
            };
            rows.push(<li key={i++} className="list-group-item">
                <Node config={childConfig} moveUUID={this.props.moveUUID} setMoveUUID={this.props.setMoveUUID} />
            </li>);
        }
        return rows;
    }
    public render() {
        const nodes = this.findChildren();
        const config = this.props.config;
        return <>
            {
                !config.isRoot && config.isbinaryTree ? (
                    config.isBinaryleft ?
                        "左子樹" : "右子樹"
                ) : < ></>
            }
            {
                config.isRoot ? <></> :
                    <div>
                        節點資料： <input type="text" value={this.state.data} onChange={(e) => this.dataChange(e)} />
                        <button type="button" className="btn btn-sm btn-warning" onClick={() => { this.dataUpdate(); }}>更新</button>
                    </div>
            }
            {
                (config.isbinaryTree && nodes.length >= 2) ? <></> : <div>
                    新增子節點： <input type="text" value={this.state.childNodeData} onChange={(e) => this.childNodeDataChange(e)} />
                    <button type="button" className="btn btn-sm btn-success" onClick={() => { this.childNodeDataCrwate(); }}>新增</button>
                </div>
            }
            {
                config.isRoot ? <></> :
                    <>
                        <button type="button" className="btn btn-danger" onClick={() => { this.deleteNode(); }}>刪除節點但保留子樹</button>
                        <button type="button" className="btn btn-danger" onClick={() => { this.deleteNodeTree(); }}>刪除節點與子樹</button>
                    </>
            }
            {this.props.moveUUID === config.uuid ?
                <button type="button" className="btn btn-info" onClick={() => { this.props.setMoveUUID(""); }}>取消移動</button>
                : (
                    this.props.moveUUID === "" ? (
                        config.isRoot ? <></> :
                            <button type="button" className="btn btn-warning" onClick={() => { this.props.setMoveUUID(config.uuid); }}>移動本節點</button>
                    )
                        :
                        <button type="button" className="btn btn-warning" onClick={() => { this.moveNodeTree(); }}>移動節點到此</button>
                )}
            {
                nodes.length > 0 ? <>
                    <hr />
                    <ul className="list-group">{this.renderChildren(nodes)}</ul>
                </> : <></>
            }
        </>;
    }

    public componentWillUnmount() {
        if (this.proxy !== null) {
            this.proxy.event.event.removeListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
        }
    }

    private findChildren() {
        const nodes: TreeDiagram_Node.Node[] = [];
        const config = this.props.config;
        const view = config.view;
        if (view !== null) {
            for (const rel of view.rels) {
                if (rel.parentUUID === config.uuid) {
                    const childIndex = view.nodes.findIndex((n) => {
                        return n.uuid === rel.childUUID;
                    });
                    if (childIndex >= 0) {
                        nodes.push(view.nodes[childIndex]);
                    }
                }
            }
        }
        // logger.info(until.inspect(view));
        // logger.info(until.inspect(nodes));
        return nodes;
    }

    private async moveNodeTree() {
        const config = this.props.config;
        const moveUUID = this.props.moveUUID;
        if (this.proxy !== null) {
            logger.info(`moveNodeTree ${config.uuid}`);
            await this.proxy.server_moveNode(moveUUID, config.uuid);
            this.props.setMoveUUID("");
        }
    }

    private async deleteNodeTree() {
        const config = this.props.config;
        if (this.proxy !== null) {
            logger.info(`deleteNodeTree ${config.uuid}`);
            await this.proxy.server_deleteNodeTree(config.uuid);
        }
    }

    private async deleteNode() {
        const config = this.props.config;
        if (this.proxy !== null) {
            logger.info(`deleteNode ${config.uuid}`);
            await this.proxy.server_deleteNode(config.uuid);
        }
    }

    private async dataUpdate() {
        const config = this.props.config;
        if (this.proxy !== null) {
            await this.proxy.server_updateNodeData(config.uuid, this.state.data);
        }
    }

    private async dataChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.setState({ data: event.target.value });
    }

    private async childNodeDataChange(event: React.ChangeEvent<HTMLInputElement>) {
        this.setState({ childNodeData: event.target.value });
    }

    private async childNodeDataCrwate() {
        const config = this.props.config;
        if (this.proxy !== null) {
            logger.info(`${config.rootUUID} ${config.uuid} ${this.state.childNodeData}`);
            await this.proxy.server_createNode(config.rootUUID, config.uuid, this.state.childNodeData);
            this.setState({ childNodeData: "" });
        }
    }

    private eventNodeUpdateListener = (uuid: string, data: string) => {
        const config = this.props.config;
        if (uuid === config.uuid && this.state.data !== data) {
            this.setState({ data });
        }
    }

    private async init() {
        this.proxy = await Proxy.GetProxy();
        this.proxy.event.event.addListener(ClientEvent.eventNodeUpdate, this.eventNodeUpdateListener);
    }
}
