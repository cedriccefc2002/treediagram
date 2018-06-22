import * as React from "react";

import { getLogger } from "log4js";

import { TreeDiagram as TreeDiagram_Tree } from "../../Ice/Tree";

import { TreeEdit } from "./TreeEdit";
import { TreeList } from "./TreeList";

const logger = getLogger();

export enum Page {
    List,
    Edit,
}

export interface IRenderState {
    treeUUID: string;
    treeType: TreeDiagram_Tree.TreeType;
    page: Page;
}

export class Render extends React.Component<{}, IRenderState> {
    public constructor(props: {}) {
        super(props);
        this.state = {
            page: Page.List,
            treeUUID: "",
            treeType: TreeDiagram_Tree.TreeType.Normal,
        };
    }
    public render() {
        switch (this.state.page) {
            case Page.List:
                return <TreeList GotoEditHandler={(uuid, type) => {
                    this.setState({ page: Page.Edit, treeType: type, treeUUID: uuid });
                }} />;
            case Page.Edit:
                return <TreeEdit uuid={this.state.treeUUID} type={this.state.treeType} CallBackHandler={() => {
                    this.setState({ page: Page.List });
                }} />;
            default:
                return <></>;
        }

    }
}
